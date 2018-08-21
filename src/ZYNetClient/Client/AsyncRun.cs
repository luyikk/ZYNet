using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;
using ZYSocket.share;

namespace ZYNet.CloudSystem.Client
{
    public class AsyncRun : AsyncRunBase, IFodyCall
    {
        public CloudClient CCloudClient { get; private set; }
        private Dictionary<Type, Type> FodyDir { get; set; }

        internal event Action<byte[]> CallSend;

        public AsyncRun(CloudClient client)
        {
            FodyDir = new Dictionary<Type, Type>();
            CCloudClient = client;
            this.Id = Id;
        }




        /// <summary>
        /// Need Nuget Install-Package Fody
        /// And Add xml file 'FodyWeavers.xml' to project
        /// context:
        /// \<?xml version="1.0" encoding="utf-8" ?\>
        /// \<Weavers\>
        /// \<Virtuosity\/\> 
        /// \</Weavers\>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>()
        {
            var interfaceType = typeof(T);
            if (!FodyDir.ContainsKey(interfaceType))
            {
                var assembly = interfaceType.Assembly;
                var implementationType = assembly.GetType(interfaceType.FullName + "_Builder_Implementation");
                if (implementationType == null)
                    throw new Exception("not find with {interfaceType.FullName} the Implementation");
                FodyDir.Add(interfaceType, implementationType);
                return (T)Activator.CreateInstance(implementationType, new Func<int, Type, object[], object>(Call));

            }
            else
            {
                return (T)Activator.CreateInstance(FodyDir[interfaceType], new Func<int, Type, object[], object>(Call));

            }
        }


        public virtual object Call(int cmd,Type needType, object[] args)
        {          

            if (needType != typeof(void))
            {

                if (!Common.IsTypeOfBaseTypeIs(needType, typeof(FiberThreadAwaiterBase)))
                {
                    throw new Exception(string.Format("Async Call Not Use Sync Mehhod"));
                }
                else
                {
                    return Func(cmd, args);
                }
            }
            else
            {
                Action(cmd, args);

                return null;
            }
        }

#if !Xamarin

        public T GetForEmit<T>()
        {
            var tmp = DispatchProxy.Create<T, SyncProxy>();
            var proxy = tmp as SyncProxy;
            proxy.Call = Call;
            return tmp;
        }




        protected virtual object Call(MethodInfo method, object[] args)
        {

            var attr = method.GetCustomAttribute(typeof(TAG), true);

            if (attr == null)
            {
                throw new FormatException(method.Name + " Is Not MethodRun Attribute");
            }
            

            if (attr is TAG run)
            {
                int cmd = run.CmdTag;

                if (method.ReturnType != typeof(void))
                {

                    if (!Common.IsTypeOfBaseTypeIs(method.ReturnType, typeof(FiberThreadAwaiterBase)))
                    {
                        throw new Exception(string.Format("Async Call Not Use Sync Mehhod"));
                    }
                    else
                    {
                        return Func(cmd, args);
                    }
                }
                else
                {
                    Action(cmd, args);

                    return null;
                }

            }
            else
                return null;
        }

#endif


        /// <summary>
        /// CALL RETURN
        /// </summary>
        /// <param name="cmdTag"></param>
        /// <param name="args"></param>
        /// <returns></returns>

        public override ResultAwatier Func(int cmdTag, params object[] args)
        {
            CallPack buffer = new CallPack()
            {
                Id = Common.MakeID,
                CmdTag = cmdTag,
                Arguments = new List<byte[]>(args.Length)
            };

            this.Id = buffer.Id;

            foreach (var item in args)
            {
                Type type = item.GetType();

                buffer.Arguments.Add(Serialization.PackSingleObject(type, item));

            }


            using (MemoryStream stream = new MemoryStream())
            {

                BinaryWriter bufflist = new BinaryWriter(stream);

                if (CCloudClient.EncodingHandler != null)
                {

                    bufflist.Write(CmdDef.CallCmd);
                    byte[] classdata = BufferFormat.SerializeObject(buffer);
                    bufflist.Write(classdata.Length);
                    bufflist.Write(classdata);

                    byte[] fdata = CCloudClient.EncodingHandler(stream.ToArray());

                    stream.Position = 0;
                    stream.SetLength(0);
                    bufflist.Write(0);
                    bufflist.Write(fdata);
                }
                else
                {
                    bufflist.Write(0);
                    bufflist.Write(CmdDef.CallCmd);
                    byte[] classdata = BufferFormat.SerializeObject(buffer);
                    bufflist.Write(classdata.Length);
                    bufflist.Write(classdata);

                }

                int l = (int)(stream.Length);

                byte[] data = BufferFormat.GetSocketBytes(l);

                stream.Position = 0;

                bufflist.Write(data);


                byte[] pdata = stream.ToArray();


                CCloudClient.AddAsyncRunBack(this, buffer.Id);

                CallSend?.Invoke(pdata);
            }

             
            if (awaiter == null)
                awaiter = new ResultAwatier();

            return base.awaiter;
        }

        /// <summary>
        /// CALL VOID
        /// </summary>
        /// <param name="cmdTag"></param>
        /// <param name="args"></param>
        public override void Action(int cmdTag, params object[] args)
        {
            CCloudClient.Sync.Action(cmdTag, args);
        }
    }
}
