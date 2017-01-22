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
    public class AsyncRun : AsyncRunBase
    {
        public CloudClient CCloudClient { get; private set; }

        internal event Action<byte[]> CallSend;

        public AsyncRun(CloudClient client)
        {
            CCloudClient = client;
            this.Id = Id;
        }


#if !Xamarin
        public T Get<T>()
        {
            var tmp = DispatchProxy.Create<T, SyncProxy>();
            var proxy = tmp as SyncProxy;
            proxy.Call = Call;
            return tmp;
        }


        protected virtual object Call(MethodInfo method, object[] args)
        {

            var attr = method.GetCustomAttribute(typeof(MethodRun), true);

            if (attr == null)
            {
                throw new FormatException(method.Name + " Is Not MethodRun Attribute");
            }


            MethodRun run = attr as MethodRun;

            if (run != null)
            {
                int cmd = run.CmdType;

                if (method.ReturnType != typeof(void))
                {
#if !COREFX
                    if (method.ReturnType.BaseType != typeof(FiberThreadAwaiterBase))
#else
                    if (method.ReturnType.GetTypeInfo().BaseType != typeof(FiberThreadAwaiterBase))
#endif
                    {
                        throw new Exception(string.Format("Async Call Not Use Sync Mehhod"));
                    }
                    else
                    {
                        return CR(cmd, args);
                    }
                }
                else
                {
                    CV(cmd, args);

                    return null;
                }

            }
            else
                return null;
        }

#endif


        public override FiberThreadAwaiter<ReturnResult> CR(int cmdTag, params object[] args)
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
#if !COREFX
                stream.Close();
#endif
                stream.Dispose();

                CCloudClient.AddAsyncRunBack(this, buffer.Id);

                if (CallSend != null)
                    CallSend(pdata);
            }

            if (awaiter == null)
                awaiter = new FiberThreadAwaiter<ReturnResult>();

            return base.awaiter;
        }

        public override void CV(int cmdTag, params object[] args)
        {
            CCloudClient.Sync.CV(cmdTag, args);
        }
    }
}
