using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;
using ZYSocket.share;
using System.IO;
using System.Diagnostics;

namespace ZYNet.CloudSystem.Client
{
    public class CloudClient
    {

        public ZYNetRingBufferPool RingBuffer { get; private set; }

        public ISyncClient Client { get; private set; }

        public string  Host { get; private set; }

        public int Port { get; private set; }
        
        public int MillisecondsTimeout { get; private set; }
        public ConcurrentDictionary<long,ReturnEventWaitHandle> SyncWaitDic { get;  set; }
        public ConcurrentDictionary<long, AsyncCalls> AsyncCallDiy { get; private set; }

        public ConcurrentDictionary<long, AsyncCalls> CallBackDiy { get; private set; }

        public ConcurrentDictionary<long, AsyncRun> AsyncRunDiy { get; private set; }

        public ModuleDictionary Module { get; private set; }


        public Func<byte[],byte[]> DecodingHandler { get; set; }

        private Func<byte[], byte[]> encodingHandler;

        public Func<byte[], byte[]> EncodingHandler { get
            {
                return encodingHandler;
            }
            set
            {
                encodingHandler = value;
                Sync.dataExtra = value;
            }
        }


        public event Action<string> Disconnect;

        public ZYSync Sync { get; private set; }

     


        public CloudClient(ISyncClient client,int millisecondsTimeout,int maxBufferLength)
        {            
            SyncWaitDic = new ConcurrentDictionary<long, ReturnEventWaitHandle>(10,10000);
            AsyncCallDiy = new ConcurrentDictionary<long, AsyncCalls>();
            CallBackDiy = new ConcurrentDictionary<long, AsyncCalls>();
            AsyncRunDiy = new ConcurrentDictionary<long, AsyncRun>();
            Client = client;
            MillisecondsTimeout = millisecondsTimeout;
            RingBuffer = new ZYNetRingBufferPool(maxBufferLength);
            Sync = new ZYSync();
            Sync.SyncSend = SendData;
            Sync.SyncSendAsWait = SendDataAsWait;
            Module = new ModuleDictionary();


        }


        public bool Connect(string host,int port)
        {
            this.Host = host;
            this.Port = port;

            if (Client.Connect(Host, port))
            {
                Client.BinaryInput += Client_BinaryInput;
                Client.Disconnect += Client_Disconnect;

                byte[] data = { 0xFF, 0xFE, 0x00, 0x00, 0x00, 0xCE, 0x00, 0xED };

                SendData(data);


                return true;
            }
            else
                return false;
        }


        public void Install(object packhandler)
        {
            Module.Install(packhandler);
        }

        internal void RemoveAsyncCall(long id)
        {
            AsyncCalls call;

            AsyncCallDiy.TryRemove(id, out call);
       
        }


        internal void AddAsyncRunBack(AsyncRun asyncalls, long id)
        {
            AsyncRunDiy.AddOrUpdate(id, asyncalls, (a, b) => asyncalls);
        }

        internal void AddAsyncCallBack(AsyncCalls asyncalls,long id)
        {
            CallBackDiy.AddOrUpdate(id, asyncalls, (a, b) => asyncalls);
        }


        public AsyncRun NewAsync()
        {
            var tmp= new AsyncRun(this);
            tmp.CallSend += SendData;
            return tmp;
        }


        private void SendData(byte[] data)
        {
            Client.Send(data);
        }

        private ReturnResult SendDataAsWait(long Id, byte[] Data)
        {

            using (ReturnEventWaitHandle wait = new ReturnEventWaitHandle(MillisecondsTimeout, false, EventResetMode.AutoReset))
            {
                if (!SyncWaitDic.TryAdd(Id, wait))
                {
                    LogAction.Log(LogType.Err,"Insert Wait Dic fail");
                    return null;
                }

                Client.Send(Data);

                wait.WaitOne();
                var value = wait.Result;
              

                if (value != null)
                {
                    if (value.Arguments == null&&!value.IsError)
                        return null;
                    else
                        return value;
                }
                else
                    return null;
            }
        }
        
        private void Client_Disconnect(string obj)
        {
            if (Disconnect != null)
                Disconnect(obj);
        }

        private void Client_BinaryInput(byte[] data)
        {
            RingBuffer.Write(data);

            byte[] pdata;

            while(RingBuffer.Read(out pdata))
            {
                DataOn(pdata);
            }
        }

     

        private void DataOn(byte[] data)
        {
            ReadBytes read = null;

            if (DecodingHandler != null)
                read = new ReadBytes(data, 4, -1, DecodingHandler);
            else
                read = new ReadBytes(data);

            int cmd;
            int length;


            if(read.ReadInt32(out length) &&read.ReadInt32(out cmd)&& read.Length == read.Length)
            {
                switch(cmd)
                {
                    case CmdDef.CallCmd:
                        {
                            CallPack tmp;

                            if (read.ReadObject<CallPack>(out tmp))
                            {
                                try
                                {
                                    CallPackRun(tmp);
                                }
                                catch (Exception er)
                                {
                                    LogAction.Log(LogType.Err,"CMD:" + tmp.CmdTag + "\r\n" + er.ToString());
                                  
                                }
                            }

                        }
                        break;
                    case CmdDef.ReturnResult:
                        {
                            ReturnResult result;

                            if (read.ReadObject<ReturnResult>(out result))
                            {

                                SetReturnValue(result);

                            }
                           
                        }
                        break;
                }
            }

        }

        private void SetReturnValue(ReturnResult result)
        {
            long idx = result.Id;

            if (CallBackDiy.ContainsKey(idx))
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
                        LogAction.Log(LogType.Err, "CMD:" + call.Cmd + " ERROR:\r\n" + er.Message);
                    }
                }
            }
            else if (AsyncRunDiy.ContainsKey(idx))
            {
                AsyncRun call;

                if (AsyncRunDiy.TryRemove(result.Id, out call))
                {
                    try
                    {
                        call.SetRet(result);
                    }
                    catch (Exception er)
                    {
                        LogAction.Log(LogType.Err, "AsynRun ID:" + result.Id + " ERROR:\r\n" + er.Message);
                    }
                }
            }
            else if (SyncWaitDic.ContainsKey(idx))
            {
                ReturnEventWaitHandle wait;

                if (SyncWaitDic.TryRemove(result.Id, out wait))
                {
                    wait.Set(result);
                }
            }           
            else
            {
                throw new InvalidOperationException("not call the Id");
            }
        }


        private void CallPackRun(CallPack pack)
        {
            if (Module.ModuleDiy.ContainsKey(pack.CmdTag))
            {
                AsyncMethodDef method = Module.ModuleDiy[pack.CmdTag];

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
                        args[x] = Serialization.UnpackSingleObject(method.ArgsType[x], pack.Arguments[i]);
                    }
                }

                if (args == null)
                {
                    LogAction.Log(LogType.Err,"Server Call To Me-> Cmd:{0} ArgsCount:{1} Args count is Error", pack.CmdTag, argcount);
                                 
                }

                if (method.IsAsync)
                {
                    if (!method.IsOut)
                    {

                        AsyncCalls _calls_ = new AsyncCalls(pack.Id, pack.CmdTag, this, method.Obj, method.methodInfo, args, false);
                        args[0] = _calls_;
                        _calls_.CallSend += SendData;
                        _calls_.Run();
                        
                        AsyncCallDiy.AddOrUpdate(pack.Id, _calls_, (a, b) => _calls_);

                    }
                    else
                    {

                        AsyncCalls _calls_ = new AsyncCalls(pack.Id, pack.CmdTag,this, method.Obj, method.methodInfo, args, true);
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
                        method.methodInfo.Invoke(method.Obj, args);
                    }
                    else
                    {
                        try
                        {
                            object res = method.methodInfo.Invoke(method.Obj, args);

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

                            LogAction.Log(LogType.Err, "Cmd:{0} ERROR:"+er.ToString(), pack.CmdTag);
                        }
                    }

                }
            }
            else
            {
                LogAction.Log(LogType.Err,"Server Call To Me-> Cmd:{0} Not Find Cmd", pack.CmdTag);
            }
        }



        private void RetrunResultData(ReturnResult result)
        {
            using (MemoryStream stream = new MemoryStream())
            {

                BinaryWriter bufflist = new BinaryWriter(stream);

                if (EncodingHandler != null)
                {

                    bufflist.Write(CmdDef.ReturnResult);
                    byte[] classdata = BufferFormat.SerializeObject(result);
                    bufflist.Write(classdata.Length);
                    bufflist.Write(classdata);

                    byte[] fdata = EncodingHandler(stream.ToArray());

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
             

                SendData(pdata);

            }

        }


        private void Close()
        {
            Client.Close();
        }


    }
}
