using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.Loggine;
using ZYSocket.share;
using Microsoft.Extensions.Logging;
using Autofac;

namespace ZYNet.CloudSystem.Client
{
    public class AsyncCalls :MessageExceptionParse, IASync
    {
        private readonly ILog Log;

        public ILoggerFactory LoggerFactory { get; private set; }

        public Result Result { get; private set; }

        public CloudClient CCloudClient { get; private set; }

        private Dictionary<Type, Type> FodyDir { get; set; }

        public bool IsOver { get; private set; }
        public bool IsError { get; private set; }
        public Exception Error { get; private set; }

        internal event Action<Result> Complete;

        internal event Action<byte[]> CallSend;

        internal Fiber _fiber { get; private set; }

        public bool IsHaveReturn { get; private set; }

        public object Obj { get; set; }

        public MethodInfo Method { get; private set; }

        public object[] Args { get; private set; }

        public int Cmd { get; private set; }

        public long Id { get; private set; }

        public ZYSync Sync => CCloudClient?.Sync;

        public IContainer Container => CCloudClient.Container;

        public CloudClient Client => CCloudClient;

        public bool IsASync => true;

        public AsyncCalls(ILoggerFactory loggerFactory, long id,int cmd, CloudClient client,Object obj, MethodInfo method,object[] args,bool ishavereturn)
        {
            IsHaveReturn = ishavereturn;
            Obj = obj;
            Method = method;
            Args = args;
            CCloudClient = client;
            Id = id;
            Cmd = cmd;
            FodyDir = new Dictionary<Type, Type>();
            Log = new DefaultLog(loggerFactory?.CreateLogger<AsyncCalls>());
            this.LoggerFactory = loggerFactory;
        }

        ~AsyncCalls()
        {
            _fiber?.Dispose();
        }


        public void Run()
        {

            async Task wrappedGhostThreadFunction()
            {
                try
                {
                    if (IsHaveReturn)
                    {
                        var res = await (dynamic)Method.Invoke(Obj, Args);
                        if (res is Result xres)
                            Result = xres;
                        else
                            Result = new Result(res);

                        Result.Id = this.Id;
                        Complete?.Invoke(Result);
                    }
                    else
                    {
                        await (Task)Method.Invoke(Obj, Args);
                    }
                }
                catch (Exception er)
                {

                    IsError = true;
                    Error = er;

                    if (IsHaveReturn)
                        Complete?.Invoke(GetExceptionResult(er, Id));

                    if (PushException(er))
                        Log.Error($"Cmd:{Cmd} Error:\r\n {Error}");
                }
                finally
                {
                    IsOver = true;
                    CCloudClient.RemoveAsyncCall(Id);
                }
            }


            _fiber = new Fiber();
            _fiber.SetAction(wrappedGhostThreadFunction);
            _fiber.Start();

        }




        #region Action
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
                    throw new FodyInstallException("not find with {interfaceType.FullName} the Implementation", (int)ErrorTag.FodyInstallErr);
                FodyDir.Add(interfaceType, implementationType);
                return (T)Activator.CreateInstance(implementationType, new Func<int, Type, object[], object>(Call));

            }
            else
            {
                return (T)Activator.CreateInstance(FodyDir[interfaceType], new Func<int, Type, object[], object>(Call));

            }
        }


        protected virtual object Call(int cmd, Type needType, object[] args)
        {

            if (needType != typeof(void))
            {

                if (!Common.IsTypeOfBaseTypeIs(needType, typeof(FiberThreadAwaiterBase)))
                {
                    throw new CallException($"Async Call Not Use Sync Mehhod CMD:{cmd}", (int)ErrorTag.CallErr);
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
                        throw new CallException($"Async Call Not Use Sync Mehhod CMD:{cmd}", (int)ErrorTag.CallErr);
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


        #endregion




        /// <summary>
        /// CALL VOID
        /// </summary>
        /// <param name="cmdTag"></param>
        /// <param name="args"></param>
        public void Action(int cmdTag, params object[] args)
        {
            Sync.Action(cmdTag, args);
        }

        /// <summary>
        /// CALL RETURN
        /// </summary>
        /// <param name="cmdTag"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ResultAwatier Func(int cmdTag, params object[] args)
        {
            CallPack buffer = new CallPack()
            {
                Id = Common.MakeID,
                CmdTag = cmdTag,
                Arguments = new List<byte[]>(args.Length)
            };

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
                   
                    bufflist.Write(buffer.Id);
                    bufflist.Write(buffer.CmdTag);
                    bufflist.Write(buffer.Arguments.Count);
                    foreach (var arg in buffer.Arguments)
                    {
                        bufflist.Write(arg.Length);
                        bufflist.Write(arg);
                    }

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
                    
                    bufflist.Write(buffer.Id);
                    bufflist.Write(buffer.CmdTag);
                    bufflist.Write(buffer.Arguments.Count);
                    foreach (var arg in buffer.Arguments)
                    {
                        bufflist.Write(arg.Length);
                        bufflist.Write(arg);
                    }

                }

                int l = (int)(stream.Length);

                byte[] data = BufferFormat.GetSocketBytes(l);

                stream.Position = 0;

                bufflist.Write(data);


                byte[] pdata = stream.ToArray();


                CCloudClient.AddAsyncCallBack(this, buffer.Id);

                CallSend?.Invoke(pdata);
            }

            return  _fiber.Read();
        }

     

        public void SetRes(Result result)
        {
            _fiber.Set(result);
        }

        public Result Res(params object[] args)
        {
            Result tmp = new Result(args)
            {
                Id = this.Id
            };
            return tmp;
        }

        public Task<Result> ResTask(params object[] args)
        {
            Result tmp = new Result(args)
            {
                Id = this.Id
            };
            return Task.FromResult(tmp);
        }
    }
}
