using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.Loggine;
using ZYSocket.share;


namespace ZYNet.CloudSystem.Client
{
    public class AsyncCalls
    {
        protected static readonly ILog Log = LogFactory.ForContext<AsyncCalls>();

        public Result Result { get; private set; }

        public CloudClient CCloudClient { get; private set; }

        public bool IsOver { get; private set; }
        public bool IsError { get; private set; }
        public Exception Error { get; private set; }

        internal event Action<Result> Complete;

        internal event Action<byte[]> CallSend;

        public Fiber _fiber { get; private set; }

        public bool IsHaveReturn { get; private set; }

        public object Obj { get; set; }


        public MethodInfo Method { get; private set; }

        public object[] Args { get; private set; }

        public int Cmd { get; private set; }

        public long Id { get; private set; }

        public ZYSync Sync => CCloudClient?.Sync;


        public AsyncCalls(long id,int cmd, CloudClient client,Object obj, MethodInfo method,object[] args,bool ishavereturn)
        {
            IsHaveReturn = ishavereturn;
            Obj = obj;
            Method = method;
            Args = args;
            CCloudClient = client;
            Id = id;
            Cmd = cmd;
        }

        ~AsyncCalls()
        {
            _fiber?.Dispose();
        }


        public void Run()
        {

            Func<Task> wrappedGhostThreadFunction = async () =>
            {
                try
                {
                    if (IsHaveReturn)
                    {
                        Result = await (Task<Result>)Method.Invoke(Obj, Args);

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
                    {
                        var nullx = new Result()
                        {
                            Id = this.Id,
                            ErrorMsg = er.ToString(),
                            ErrorId = er.HResult                            
                        };

                        Complete?.Invoke(nullx);
                    }

                    Log.Error($"Cmd:{Cmd} Error:\r\n {Error}");
                }
                finally
                {
                    IsOver = true;
                    CCloudClient.RemoveAsyncCall(Id);
                }
            };


            _fiber = new Fiber();
            _fiber.SetAction(wrappedGhostThreadFunction);
            _fiber.Start();

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
        /// CALL VOID
        /// </summary>
        /// <param name="cmdTag"></param>
        /// <param name="args"></param>
        public void Action(int cmdTag, params object[] args)
        {
            Sync.CV(cmdTag, args);
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
    }
}
