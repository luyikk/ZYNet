using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;
using ZYSocket.share;
using ZYSocket.ZYCoroutinesin;

namespace ZYNet.CloudSystem.Client
{
    public class AsyncCalls
    {
        public ReturnResult Result { get; private set; }

        public CloudClient CCloudClient { get; private set; }

        public bool IsOver { get; private set; }
        public bool IsError { get; private set; }
        public Exception Error { get; private set; }

        internal event Action<ReturnResult> Complete;

        internal event Action<byte[]> CallSend;

        public Fiber fiber { get; private set; }

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

        public void Run()
        {

            Func<Task> wrappedGhostThreadFunction = async () =>
            {
                try
                {
                    if (IsHaveReturn)
                    {
                        Result = await (Task<ReturnResult>)Method.Invoke(Obj, Args);

                        if (Complete != null)
                            Complete(Result);                       
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
                        var nullx = new ReturnResult();
                        nullx.Id = this.Id;

                        if (Complete != null)
                            Complete(nullx);
                    }

                    LogAction.Log(LogType.Err, "Cmd:" + Cmd + " Error:\r\n" + Error.ToString());
                }
                finally
                {
                    IsOver = true;
                    CCloudClient.RemoveAsyncCall(Id);
                }
            };


            fiber = new Fiber();
            fiber.SetAction(wrappedGhostThreadFunction);
            fiber.Start();

        }



        public void CV(int cmdTag, params object[] args)
        {
            Sync.CV(cmdTag, args);
        }

        public FiberThreadAwaiter<ReturnResult> CR(int cmdTag, params object[] args)
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
                stream.Close();
                stream.Dispose();

                CCloudClient.AddAsyncCallBack(this, buffer.Id);

                if (CallSend != null)
                    CallSend(pdata);
            }

            return fiber.Read<ReturnResult>();
        }

        public void SetRet(ReturnResult result)
        {
            fiber.Set<ReturnResult>(result);
        }

        public ReturnResult RET(params object[] args)
        {
            ReturnResult tmp = new ReturnResult(args);
            tmp.Id = this.Id;
            return tmp;
        }
    }
}
