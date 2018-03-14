using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;
using ZYSocket.share;
using System.IO;
using ZYSocket.AsyncSend;
using ZYNet.CloudSystem.Loggine;

namespace ZYNet.CloudSystem.Server
{
    public class ASyncToken:ZYSync
    {
        protected static readonly ILog Log = LogFactory.ForContext<ASyncToken>();

        public SocketAsyncEventArgs Asyn { get; private set; }
        public ZYNetRingBufferPool Stream { get; private set; }

        public CloudServer CurrentServer { get;  private set; }

        public ConcurrentDictionary<long, AsyncCalls> AsyncCallDiy { get; private set; }
        public ConcurrentDictionary<long, AsyncCalls> CallBackDiy { get; private set; }

        public event Action<ASyncToken,string> UserDisconnect;

        public AsyncSend Sendobj { get; private set; }

        public object UserToken { get; set; }

        public bool IsValidate { get; set; }

        public T Token<T>()
        {
            if (UserToken == null)
                return default(T);
            else
                return (T)UserToken;
        }



        public ASyncToken(SocketAsyncEventArgs asynca, CloudServer server, int MaxBuffsize)
        {
            this.Asyn = asynca;
            Stream = new ZYNetRingBufferPool(MaxBuffsize);
            CurrentServer = server;
            this.DataExtra = server.EcodeingHandler;
            Sendobj = new AsyncSend(asynca.AcceptSocket);
        }

        internal void RemoveAsyncCall(long id)
        {           

            if(!AsyncCallDiy.TryRemove(id, out AsyncCalls call))
            {
               
            }          
         
        }


        internal async void AddAsyncCallBack(AsyncCalls asyncalls, long id)
        {
            if (CallBackDiy == null)
                CallBackDiy = new ConcurrentDictionary<long, AsyncCalls>();

            CallBackDiy.AddOrUpdate(id, asyncalls, (a, b) => asyncalls);

            if (asyncalls.CurrentServer.CheckTimeOut)
            {

                await Task.Delay(asyncalls.CurrentServer.ReadOutTime);

                if (CallBackDiy.ContainsKey(id))
                {
                    if (CallBackDiy.TryRemove(id, out AsyncCalls value))
                    {
                        var timeout = new Result()
                        {
                            Id = id,
                            ErrorMsg = "Server call client time out",
                            ErrorId = -101
                        };

                        try
                        {
                            value.SetRes(timeout);
                        }
                        catch (Exception er)
                        {
                            Log.Error($"CMD:{value.Cmd} Id:{value.Id} ERROR:\r\n{er.Message}");
                        }
                    }
                }
            }

        }


        internal void Write(byte[] data,int offset,int count)
        {
            Stream.Write(data,offset,count);        

            while(Stream.Read(out byte[] pdata))
            {
                DataOn(pdata);
            }

        }

        internal void Disconnect(string message)
        {
            if(AsyncCallDiy!=null)
                AsyncCallDiy.Clear();

            if (CallBackDiy != null)
            {

                foreach (var item in CallBackDiy.Values)
                {
                    Result disconn = new Result
                    {
                        Id = item.Id,
                        ErrorId = -1,
                        ErrorMsg = "Disconnect",
                        Arguments = new List<byte[]>()
                    };
                    item.SetRes(disconn);
                }


                CallBackDiy.Clear();
            }

            UserDisconnect?.Invoke(this, message);
        }


        public void Disconnect()
        {
            var server = CurrentServer?.Server;
            var socket = Asyn?.AcceptSocket;
            if (server != null && socket != null)
                server.Disconnect(Asyn.AcceptSocket);
        }

        public void DataOn(byte[] data)
        {
        

            ReadBytes read = null;

            if (CurrentServer.DecodeingHandler != null)
                read = new ReadBytes(data, 4, -1, CurrentServer.DecodeingHandler);
            else
                read = new ReadBytes(data);


            if (read.ReadInt32(out int length) && read.ReadInt32(out int cmd) && read.Length == length)
            {
                switch (cmd)
                {
                    case CmdDef.CallCmd:
                        {

                            if (read.ReadObject<CallPack>(out CallPack tmp))
                            {
                                try
                                {
                                    CallPackRun(tmp);
                                }
                                catch (Exception er)
                                {
                                   Log.Error($"CMD:{tmp.CmdTag} Error:\r\n {er}");
                                }
                            }
                        }
                        break;
                    case CmdDef.ReturnResult:
                        {

                            if (read.ReadObject<Result>(out Result result))
                            {
                                if (CallBackDiy.ContainsKey(result.Id))
                                {

                                    if (CallBackDiy.TryRemove(result.Id, out AsyncCalls call))
                                    {
                                        try
                                        {
                                            call.SetRes(result);

                                        }
                                        catch (Exception er)
                                        {
                                            Log.Error($"Cmd:{call.Cmd} Error:\r\n {er}");
                                        }
                                    }
                                }
                            }
                        }
                        break;

                }
            }
        }

        protected override void SendData(byte[] data)
        {
            CurrentServer.Send(Sendobj, data);
        }

        protected override Result SendDataAsWait(long Id, byte[] Data)
        {
            throw new InvalidOperationException("Server Sync CR Not Use!! please use CV");
        }



        private void ResrunResultData(Result result)
        {
            using (MemoryStream stream = new MemoryStream())
            {

                BinaryWriter bufflist = new BinaryWriter(stream);

                if (DataExtra != null)
                {

                    bufflist.Write(CmdDef.ReturnResult);
                    byte[] classdata = BufferFormat.SerializeObject(result);
                    bufflist.Write(classdata.Length);
                    bufflist.Write(classdata);

                    byte[] fdata = DataExtra(stream.ToArray());

                    stream.Position = 0;
                    stream.SetLength(0);
                    bufflist.Write(0);
                    bufflist.Write(fdata);
                }
                else
                {
                    bufflist.Write(0);
                    bufflist.Write(CmdDef.ReturnResult);
                    byte[] classdata = BufferFormat.SerializeObject(result);
                    bufflist.Write(classdata.Length);
                    bufflist.Write(classdata);

                }

                int l = (int)(stream.Length);

                byte[] data = BufferFormat.GetSocketBytes(l);

                stream.Position = 0;

                bufflist.Write(data);


                byte[] pdata = stream.ToArray();

                SendData(pdata);

            }

        }


        public AsyncCalls MakeAsync(AsyncCalls async)
        {
            AsyncCalls tmp = new AsyncCalls(this, async._fiber);
            tmp.CallSend += SendData;
            return tmp;
        }



        private  void CallPackRun(CallPack pack)
        {
            if(CurrentServer.CallsMethods.ContainsKey(pack.CmdTag))
            {
                if(AsyncCallDiy==null)
                    AsyncCallDiy = new ConcurrentDictionary<long, AsyncCalls>();

                var method = CurrentServer.CallsMethods[pack.CmdTag];

               

                object[] args = null;

                int argcount = 0;

                if (pack.Arguments != null)
                    argcount = pack.Arguments.Count;

                if (method.ArgsType.Length > 0 && method.ArgsType.Length == (argcount + 1))
                {
                    args = new object[method.ArgsType.Length];

                    args[0] = this;              
                    int x = 1;
                    for (int i = 0; i < (method.ArgsType.Length - 1); i++)
                    {
                        x = i + 1;
                        args[x] =  Serialization.UnpackSingleObject(method.ArgsType[x], pack.Arguments[i]);
                    }
                }

                if (args == null)
                {
                    Log.ErrorFormat("Clent Call Cmd:{0} ArgsCount:{1} Args count is Error", pack.CmdTag, argcount);
                    return;
                }
                if (method.IsAsync)
                {
                    if (!method.IsOut)
                    {

                        AsyncCalls _calls_ = new AsyncCalls(pack.Id, pack.CmdTag, this, method.MethodInfo, args, false);
                        args[0] = _calls_;
                        _calls_.CallSend += SendData;                   
                        AsyncCallDiy.AddOrUpdate(pack.Id, _calls_, (a, b) => _calls_);
                        _calls_.Run();

                    }
                    else
                    {

                        AsyncCalls _calls_ = new AsyncCalls(pack.Id, pack.CmdTag, this, method.MethodInfo, args, true);
                        args[0] = _calls_;
                        _calls_.CallSend += SendData;
                        _calls_.Complete += ResrunResultData;  
                        AsyncCallDiy.AddOrUpdate(pack.Id, _calls_, (a, b) => _calls_);
                        _calls_.Run();
                    }
                }
                else //SYNC
                {
                    if (!method.IsOut)
                    {
                        method.MethodInfo.Invoke(null, args);
                    }
                    else
                    {
                        try
                        {
                            object res = method.MethodInfo.Invoke(null, args);

                            if (res != null)
                            {
                                Result tmp = new Result(res)
                                {
                                    Id = pack.Id
                                };
                                ResrunResultData(tmp);

                            }
                        }
                        catch (Exception er)
                        {
                            Result tmp = new Result
                            {
                                Id = pack.Id
                            };
                            ResrunResultData(tmp);

                            Log.Error($"Cmd:{pack.CmdTag} ERROR:{er}");
                        }
                    }

                }
            }
            else
            {
                Log.Error($"Clent Call Cmd:{pack.CmdTag} Not Find");
            }
        }


        

      
    }
}
