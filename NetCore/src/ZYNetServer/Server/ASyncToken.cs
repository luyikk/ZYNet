using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;
using ZYSocket.share;
using ZYSocket.ZYCoroutinesin;
using System.IO;

namespace ZYNet.CloudSystem.Server
{
    public class ASyncToken:ZYSync
    {
        public SocketAsyncEventArgs Asyn { get; private set; }
        public ZYNetRingBufferPool Stream { get; private set; }

        public CloudServer CurrentServer { get;  private set; }

        public ConcurrentDictionary<long, AsyncCalls> AsyncCallDiy { get; private set; }
        public ConcurrentDictionary<long, AsyncCalls> CallBackDiy { get; private set; }

        public event Action<ASyncToken,string> UserDisconnect;



        public object UserToken { get; set; }


        public ASyncToken(SocketAsyncEventArgs asynca, CloudServer server, int MaxBuffsize)
        {
            this.Asyn = asynca;
            Stream = new ZYNetRingBufferPool(MaxBuffsize);
            CurrentServer = server;
            this.dataExtra = server.EcodeingHandler;
        }

        internal void RemoveAsyncCall(long id)
        {
            AsyncCalls call;

            AsyncCallDiy.TryRemove(id, out call);

            //foreach (var item in CallBackDiy.Where(p => p.Value == call))
            //{
            //    AsyncCalls x;
            //    CallBackDiy.TryRemove(item.Key, out x);
            //}
        }


        internal void AddAsyncCallBack(AsyncCalls asyncalls, long id)
        {
            if (CallBackDiy == null)
                CallBackDiy = new ConcurrentDictionary<long, AsyncCalls>();

            CallBackDiy.AddOrUpdate(id, asyncalls, (a, b) => asyncalls);
        }


        internal void Write(byte[] data,int offset,int count)
        {
            Stream.Write(data,offset,count);

            byte[] pdata;

            while(Stream.Read(out pdata))
            {
                DataOn(pdata);
            }

        }

        internal void Disconnect(string message)
        {
            if(AsyncCallDiy!=null)
                AsyncCallDiy.Clear();
            if(CallBackDiy!=null)
                CallBackDiy.Clear();

            if (UserDisconnect != null)
                UserDisconnect(this,message);
        }

        public void DataOn(byte[] data)
        {
        

            ReadBytes read = null;

            if (CurrentServer.DecodeingHandler != null)
                read = new ReadBytes(data, 4, -1, CurrentServer.DecodeingHandler);
            else
                read = new ReadBytes(data);

            int cmd;
            int length;

            if(read.ReadInt32(out length) &&read.ReadInt32(out cmd)&& read.Length== length)
            {
                switch(cmd)
                {
                    case CmdDef.CallCmd:
                        {
                            CallPack tmp;

                            if(read.ReadObject<CallPack>(out tmp))
                            {
                                try
                                {
                                    CallPackRun(tmp);
                                }
                                catch (Exception er)
                                {
                                    LogAction.Log(LogType.Err,"CMD:" +tmp.CmdTag + "\r\n" + er.ToString());
                                }
                            }
                        }
                        break;
                    case CmdDef.ReturnResult:
                        {
                            ReturnResult result;

                            if (read.ReadObject<ReturnResult>(out result))
                            {
                                if (CallBackDiy.ContainsKey(result.Id))
                                {
                                    AsyncCalls call;

                                    if (CallBackDiy.TryRemove(result.Id, out call))
                                    {
                                        try
                                        {
                                            call.SetRet(result);
                                          
                                        }
                                        catch (Exception er)
                                        {
                                            LogAction.Log(LogType.Err, "Cmd:" + call.Cmd + " Error:\r\n" + er.ToString());
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
            CurrentServer.Send(Asyn.AcceptSocket, data);
        }

        protected override ReturnResult SendDataAsWait(long Id, byte[] Data)
        {
            throw new InvalidOperationException("Server Sync CR Not Use!! please use CV");
        }



        private void RetrunResultData(ReturnResult result)
        {
            using (MemoryStream stream = new MemoryStream())
            {

                BinaryWriter bufflist = new BinaryWriter(stream);

                if (dataExtra != null)
                {

                    bufflist.Write(CmdDef.ReturnResult);
                    byte[] classdata = BufferFormat.SerializeObject(result);
                    bufflist.Write(classdata.Length);
                    bufflist.Write(classdata);

                    byte[] fdata = dataExtra(stream.ToArray());

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
#if !COREFX
                stream.Close();
#endif
                stream.Dispose();

                SendData(pdata);

            }

        }


        public AsyncCalls MakeAsync(AsyncCalls async)
        {
            AsyncCalls tmp = new AsyncCalls(this, async.fiber);
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
                    LogAction.Log(LogType.Err,"Clent Call Cmd:{0} ArgsCount:{1} Args count is Error", pack.CmdTag, argcount);
                    return;
                }
                if (method.IsAsync)
                {
                    if (!method.IsOut)
                    {

                        AsyncCalls _calls_ = new AsyncCalls(pack.Id, pack.CmdTag, this, method.methodInfo, args, false);
                        args[0] = _calls_;
                        _calls_.CallSend += SendData;
                        _calls_.Run();

                     

                        AsyncCallDiy.AddOrUpdate(pack.Id, _calls_, (a, b) => _calls_);

                    }
                    else
                    {

                        AsyncCalls _calls_ = new AsyncCalls(pack.Id, pack.CmdTag, this, method.methodInfo, args, true);
                        args[0] = _calls_;
                        _calls_.CallSend += SendData;
                        _calls_.Complete += RetrunResultData;
                        _calls_.Run();

                      

                        AsyncCallDiy.AddOrUpdate(pack.Id, _calls_, (a, b) => _calls_);
                    }
                }
                else //SYNC
                {
                    if (!method.IsOut)
                    {
                        method.methodInfo.Invoke(null, args);
                    }
                    else
                    {
                        try
                        {
                            object res = method.methodInfo.Invoke(null, args);

                            if (res != null)
                            {
                                ReturnResult tmp = new ReturnResult(res);
                                tmp.Id = pack.Id;
                                RetrunResultData(tmp);

                            }
                        }
                        catch (Exception er)
                        {
                            ReturnResult tmp = new ReturnResult();
                            tmp.Id = pack.Id;
                            RetrunResultData(tmp);

                            LogAction.Log(LogType.Err, "Cmd:{0} ERROR:" + er.ToString(), pack.CmdTag);
                        }
                    }

                }
            }
            else
            {
                LogAction.Log(LogType.Err,"Clent Call Cmd:{0} Not Find", pack.CmdTag);
            }
        }


        

      
    }
}
