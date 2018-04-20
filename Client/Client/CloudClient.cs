using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using ZYNet.CloudSystem.Frame;
using ZYSocket.share;
using System.IO;
using ZYNet.CloudSystem.Loggine;
using System.Threading.Tasks;

namespace ZYNet.CloudSystem.Client
{
    public class CloudClient
    {
      

        protected static readonly ILog Log = LogFactory.ForContext<CloudClient>();

        protected WaitHandle _ReadWaitCheck = new EventWaitHandle(false, EventResetMode.AutoReset);

        public ZYNetRingBufferPool RingBuffer { get; private set; }

        public ISyncClient Client { get; private set; }

        public string  Host { get; private set; }

        public int Port { get; private set; }


        private bool _checkAasyncTimeOut;
        public bool CheckAsyncTimeOut {
            get
            {
                return _checkAasyncTimeOut;
            }
            set
            {
                if (!value)
                    AsyncWaitTimeOut.Clear();

                _checkAasyncTimeOut = value;
            }
        }

        public int MillisecondsTimeout { get; private set; }
        public int MaxBufferLength { get; private set; }
        public ConcurrentDictionary<long,ReturnEventWaitHandle> SyncWaitDic { get;  set; }
        public ConcurrentDictionary<long, AsyncCalls> AsyncCallDiy { get; private set; }

        public ConcurrentDictionary<long, AsyncCalls> CallBackDiy { get; private set; }

        public ConcurrentDictionary<long, AsyncRun> AsyncRunDiy { get; private set; }

        public List<KeyValuePair<long,DateTime>> AsyncWaitTimeOut { get; private set; }

        public ModuleDictionary Module { get; private set; }
        public bool IsClose { get; private set; }
        public Func<byte[],byte[]> DecodingHandler { get; set; }

        private Func<byte[], byte[]> encodingHandler;

        public Func<byte[], byte[]> EncodingHandler { get
            {
                return encodingHandler;
            }
            set
            {
                encodingHandler = value;
                Sync.DataExtra = value;
            }
        }


        public event Action<string> Disconnect;

        public ZYSync Sync { get; private set; }

        public void Close()
        {
            IsClose = true;
            AsyncWaitTimeOut.Clear();
            Client.Close();
            Module.ModuleDiy.Clear();
            AsyncRunDiy.Clear();
            CallBackDiy.Clear();
            AsyncCallDiy.Clear();
            SyncWaitDic.Clear();
           
        }


        public CloudClient(ISyncClient client, int millisecondsTimeout, int maxBufferLength)
        {
            SyncWaitDic = new ConcurrentDictionary<long, ReturnEventWaitHandle>(10, 10000);
            AsyncCallDiy = new ConcurrentDictionary<long, AsyncCalls>();
            CallBackDiy = new ConcurrentDictionary<long, AsyncCalls>();
            AsyncRunDiy = new ConcurrentDictionary<long, AsyncRun>();
            AsyncWaitTimeOut = new List<KeyValuePair<long, DateTime>>();
            Client = client;
            MillisecondsTimeout = millisecondsTimeout;
            MaxBufferLength = maxBufferLength;
            Sync = new ZYSync()
            {
                SyncSend = SendData,
                SyncSendAsWait = SendDataAsWait
            };
            Module = new ModuleDictionary();
            IsClose = false;
            ThreadPool.RegisterWaitForSingleObject(_ReadWaitCheck, new WaitOrTimerCallback(checkAsyncTimeOut), null, 100, true);
        }

        private void checkAsyncTimeOut(object o,bool b)
        {
            int timeSleep = 1;

            try
            {
                if (!CheckAsyncTimeOut || AsyncWaitTimeOut.Count == 0)
                    timeSleep = 1000;
                else
                {
                    var res = AsyncWaitTimeOut.FindAll(p => p.Value < DateTime.Now);

                    if (res.Count == 0)
                        timeSleep = 200;
                    else
                    {
                        foreach (var item in res)
                        {
                            long id = item.Key;
                            if (AsyncRunDiy.ContainsKey(id))
                            {
                                if (AsyncRunDiy.TryRemove(id, out AsyncRun value))
                                {
                                    Task.Run(() =>
                                        {

                                            var timeout = new Result()
                                            {
                                                Id = id,
                                                ErrorMsg = "run time out",
                                                ErrorId = -101
                                            };

                                            try
                                            {
                                                value.SetRet(timeout);
                                            }
                                            catch (Exception er)
                                            {
                                                Log.Error($"Id:{value.Id} ERROR:\r\n{er.Message}");
                                            }

                                        });
                                }
                            }

                            AsyncWaitTimeOut.Remove(item);
                        }
                    }
                }

            }
            catch(Exception er)
            {
                Log.Error($"ERROR:\r\n{er.ToString()}");
            }
            finally
            {
                if(!IsClose)
                    ThreadPool.RegisterWaitForSingleObject(_ReadWaitCheck, new WaitOrTimerCallback(checkAsyncTimeOut), null, timeSleep, true);
            }

        }


        public bool Connect(string host,int port)
        {
            this.Host = host;
            this.Port = port;

            if (Client.Connect(Host, port))
            {
                RingBuffer = new ZYNetRingBufferPool(MaxBufferLength); 
                Client.BinaryInput += Client_BinaryInput;
                Client.Disconnect += Client_Disconnect;

                byte[] data = { 0xFF, 0xFE, 0x00, 0x00, 0x00, 0xCE, 0x00, 0xED };

                SendData(data);


                return true;
            }
            else
                return false;
        }

        public async Task<bool> ConnectAsync(string host,int port)
        {
            this.Host = host;
            this.Port = port;
          
            return await Task.Run<bool>(() =>
            {
                if (Client.Connect(Host, port))
                {
                    RingBuffer = new ZYNetRingBufferPool(MaxBufferLength); 
                    Client.BinaryInput += Client_BinaryInput;
                    Client.Disconnect += Client_Disconnect;

                    byte[] data = { 0xFF, 0xFE, 0x00, 0x00, 0x00, 0xCE, 0x00, 0xED };

                    SendData(data);

                    return true;
                }
                else
                    return false;

            });
        }


        public CloudClient Install(object packhandler)
        {
            Module.Install(packhandler);
            return this;
        }

        internal void RemoveAsyncCall(long id)
        {           
            AsyncCallDiy.TryRemove(id, out AsyncCalls call);
       
        }


        internal void AddAsyncRunBack(AsyncRun asyncalls, long id)
        {
            AsyncRunDiy.AddOrUpdate(id, asyncalls, (a, b) => asyncalls);

            if (CheckAsyncTimeOut)
            {
                KeyValuePair<long, DateTime> tot = new KeyValuePair<long, DateTime>(id, DateTime.Now.AddMilliseconds(MillisecondsTimeout));
                AsyncWaitTimeOut.Add(tot);
            }
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

        private Result SendDataAsWait(long Id, byte[] Data)
        {

            using (ReturnEventWaitHandle wait = new ReturnEventWaitHandle(MillisecondsTimeout, false, EventResetMode.AutoReset))
            {
                if (!SyncWaitDic.TryAdd(Id, wait))
                {
                    Log.Error("Insert Wait Dic fail");
                   
                }

                Client.Send(Data);

                if(!wait.WaitOne())
                    throw new TimeoutException("Call Time Out");

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
            Disconnect?.Invoke(obj);
            RingBuffer = null;
        }

        private void Client_BinaryInput(byte[] data)
        {
            RingBuffer.Write(data);
                       

            while(RingBuffer.Read(out byte[] pdata))
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
                       
            
            if(read.ReadInt32(out int length) &&read.ReadInt32(out int cmd)&& read.Length == read.Length)
            {
                switch(cmd)
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
                                    Log.Error($"CMD:{tmp.CmdTag} Error:\r\n{er}");
                                  
                                }
                            }

                        }
                        break;
                    case CmdDef.ReturnResult:
                        {
                          
                            if (read.ReadObject<Result>(out Result result))
                            {
                                SetReturnValue(result);
                            }
                           
                        }
                        break;
                }
            }

        }

        private void SetReturnValue(Result result)
        {
            long idx = result.Id;

            if (CallBackDiy.ContainsKey(idx))
            {               
                if (CallBackDiy.TryRemove(result.Id, out AsyncCalls call))
                {
                    try
                    {
                        call.SetRet(result);
                    }
                    catch (Exception er)
                    {
                        Log.Error($"CMD:{call.Cmd} ERROR:\r\n{er.Message}");
                    }
                }
            }
            else if (AsyncRunDiy.ContainsKey(idx))
            {                 
                if (AsyncRunDiy.TryRemove(result.Id, out AsyncRun call))
                {
                    try
                    {
                        call.SetRet(result);
                    }
                    catch (Exception er)
                    {
                        Log.Error($"AsynRun ID:{result.Id} ERROR:\r\n{er.Message}");
                    }
                }
            }
            else if (SyncWaitDic.ContainsKey(idx))
            {               

                if (SyncWaitDic.TryRemove(result.Id, out ReturnEventWaitHandle wait))
                {
                    wait.Set(result);
                }
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
                    Log.ErrorFormat("Server Call To Me-> Cmd:{0} ArgsCount:{1} Args count is Error", pack.CmdTag, argcount);
                                 
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
                                var tmp = new Result(res)
                                {
                                    Id = pack.Id
                                };
                              
                                RetrunResultData(tmp);
                            }
                        }
                        catch (Exception er)
                        {
                            var tmp = new Result()
                            {
                                Id = pack.Id
                            };
                            RetrunResultData(tmp);

                            Log.Error($"Cmd:{pack.CmdTag} ERROR:{er.ToString()}");
                        }
                    }

                }
            }
            else
            {
                Log.Error($"Server Call To Me-> Cmd:{pack.CmdTag} Not Find Cmd");
            }
        }



        private void RetrunResultData(Result result)
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
             

                SendData(pdata);

            }

        }


      

    }
}
