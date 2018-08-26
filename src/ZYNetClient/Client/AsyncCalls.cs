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
    public class AsyncCalls :MessageExceptionParse
    {
        protected static readonly ILog Log = LogFactory.ForContext<AsyncCalls>();

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


        public AsyncCalls(long id,int cmd, CloudClient client,Object obj, MethodInfo method,object[] args,bool ishavereturn)
        {
            IsHaveReturn = ishavereturn;
            Obj = obj;
            Method = method;
            Args = args;
            CCloudClient = client;
            Id = id;
            Cmd = cmd;
            FodyDir = new Dictionary<Type, Type>();
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

                    if(PushException(er))
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
                    //byte[] classdata = BufferFormat.SerializeObject(buffer);
                    //bufflist.Write(classdata.Length);
                    //bufflist.Write(classdata);
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
