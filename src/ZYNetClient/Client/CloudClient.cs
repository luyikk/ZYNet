using System;
using System.Collections.Concurrent;
using System.Threading;
using ZYNet.CloudSystem.Frame;
using ZYSocket.share;
using System.IO;
using ZYNet.CloudSystem.Loggine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Autofac;
using ZYNet.CloudSystem.Client.Options;

namespace ZYNet.CloudSystem.Client
{
    public class CloudClient:MessageExceptionParse, IASync,IDisposable
    {


        public readonly ILog Log;
        public ILoggerFactory LoggerFactory { get; private set; }


        public IConnectionManager ClientManager { get; private set; }

        public string  Host { get; private set; }

        public int Port { get; private set; }

        public IContainer Container { get; private set; }

        public bool IsCheckAsyncTimeOut { get; set; }
        public int MillisecondsTimeout { get; private set; }     
        public int MaxBufferLength { get; private set; }
        public ConcurrentDictionary<long,ReturnEventWaitHandle> SyncWaitDic { get;  set; }
        public ConcurrentDictionary<long, AsyncCalls> AsyncCallDiy { get; private set; }

        public ConcurrentDictionary<long, AsyncCalls> CallBackDiy { get; private set; }

        public ConcurrentDictionary<long, AsyncRun> AsyncRunDiy { get; private set; }

        public List<KeyValuePair<long, DateTime>> AsyncWaitTimeOut { get; private set; }

        private Dictionary<Type, Type> FodyDir { get; set; }

        public IModuleDictionary Module { get; private set; }

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

        public CloudClient Client => this;

        public bool IsASync => false;

        public CloudClient(IContainer container)
        {
            SyncWaitDic = new ConcurrentDictionary<long, ReturnEventWaitHandle>(10, 10000);
            AsyncCallDiy = new ConcurrentDictionary<long, AsyncCalls>();
            CallBackDiy = new ConcurrentDictionary<long, AsyncCalls>();
            AsyncRunDiy = new ConcurrentDictionary<long, AsyncRun>();
            AsyncWaitTimeOut = new List<KeyValuePair<long, DateTime>>();
            FodyDir = new Dictionary<Type, Type>();

            ClientManager = container.Resolve<IConnectionManager>();
            ClientManager.BinaryInput += DataOn;
            ClientManager.Disconnect += p => Disconnect?.Invoke(p);
            MillisecondsTimeout = container.Resolve<TimeOutOptions>().MillisecondsTimeout;
            IsCheckAsyncTimeOut = container.Resolve<TimeOutOptions>().IsCheckAsyncTimeOut;

            Sync = new ZYSync()
            {
                SyncSend = SendData,
                SyncSendAsWait = SendDataAsWait
            };

          
            Module = container.Resolve<IModuleDictionary>();
            IsClose = false;

            LoggerFactory = container.Resolve<ILoggerFactory>();
            Log = new DefaultLog(LoggerFactory.CreateLogger<CloudClient>());

            Task.Run(new Action(CheckAsyncTimeOut));
        }

     

        private async void CheckAsyncTimeOut()
        {
            while (true)
            {
                if (IsClose)
                    break;

                int timeSleep = 1;

                try
                {
                    if (!IsCheckAsyncTimeOut || AsyncWaitTimeOut.Count == 0)
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
                                        await Task.Run(() =>
                                        {
                                            try
                                            {
                                                value.SetRet(GetExceptionResult("run time out",(int)ErrorTag.TimeOut,id));
                                            }
                                            catch (Exception er)
                                            {
                                                if(PushException(new SetResultException(er.Message, (int)ErrorTag.SetErr, er)))
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
                catch (Exception er)
                {
                    if(PushException(er))
                        Log.Error($"ERROR:\r\n{er.ToString()}");
                }
                finally
                {
                    await Task.Delay(timeSleep);
                }
            }
        }

        public bool Init(string host,int port)
        {
            return  ClientManager.Install(host, port);
        }

        public Task<bool> InitAsync(string host, int port)
        {
            return ClientManager.InstallAsync(host, port);
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

            if (IsCheckAsyncTimeOut)
            {
                KeyValuePair<long, DateTime> tot = new KeyValuePair<long, DateTime>(id, DateTime.Now.AddMilliseconds(MillisecondsTimeout));
                AsyncWaitTimeOut.Add(tot);
            }
        }

        internal void AddAsyncCallBack(AsyncCalls asyncalls,long id)
        {
            CallBackDiy.AddOrUpdate(id, asyncalls, (a, b) => asyncalls);
            
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
                    throw new FodyInstallException("not find with {interfaceType.FullName} the Implementation",(int)ErrorTag.FodyInstallErr);
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

                    if (needType == typeof(Result))
                    {
                        return Sync.Func(cmd, args);
                    }
                    else
                    {
                        try
                        {
                            var res = Sync.Func(cmd, args);

                            if (res is null)
                                return null;

                            if (res.IsError)
                                throw new CallException(res.ErrorMsg,res.ErrorId);

                            return res.First?.Value(needType);
                          
                        }
                        catch (Exception er)
                        {
                            throw new ReturnTypeException(string.Format("Return Type of {0} Error", needType),(int)ErrorTag.ReturnTypeErr, er);
                        }
                    }

                }
                else
                {
                    return NewAsync().Func(cmd, args);
                }
            }
            else
            {
                Sync.Action(cmd, args);

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
                        if (method.ReturnType == typeof(Result))
                        {
                            return Sync.Func(cmd, args);
                        }
                        else
                        {
                            try
                            {
                                var res = Sync.Func(cmd, args);

                                if (res is null)
                                    return null;

                                if (res.IsError)
                                    throw new CallException(res.ErrorMsg,res.ErrorId);

                                return res?.First?.Value(method.ReturnType);
                            }
                            catch (Exception er)
                            {
                                throw new ReturnTypeException(string.Format("Return Type of {0} Error", method.ReturnType),(int)ErrorTag.ReturnTypeErr, er);
                            }
                        }
                    }
                    else
                    {
                        return NewAsync().Func(cmd, args);
                    }
                }
                else
                {
                    Sync.Action(cmd, args);

                    return null;
                }

            }
            else
                return null;
        }

#endif


        #endregion

        public AsyncRun NewAsync()
        {
            AsyncRun run1 = new AsyncRun(this);
            run1.CallSend += new Action<byte[]>(this.SendData);
            return run1;
        }

        public void Action(int cmdTag, params object[] args)
        {
            Sync.Action(cmdTag, args);
        }

        public ResultAwatier Func(int cmdTag, params object[] args)
        {
            return NewAsync().Func(cmdTag, args);
        }


        private void SendData(byte[] data)
        {
            ClientManager.SendData(data);
        }

        private Result SendDataAsWait(long Id, byte[] Data)
        {

            using (ReturnEventWaitHandle wait = new ReturnEventWaitHandle(this.LoggerFactory, MillisecondsTimeout, false, EventResetMode.AutoReset))
            {
                if (!SyncWaitDic.TryAdd(Id, wait))
                {
                    Log.Error("Insert Wait Dic fail");
                   
                }

                ClientManager.SendData(Data);

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

                            CallPack call = new CallPack();

                            try
                            {
                               
                                call.Id = read.ReadInt64();
                                call.CmdTag = read.ReadInt32();
                                call.Arguments = new List<byte[]>();
                                var lengt = read.ReadInt32();
                                for (int i = 0; i < lengt; i++)
                                {
                                    call.Arguments.Add(read.ReadByteArray());
                                }

                                CallPackRun(call);
                            }
                            catch (Exception er)
                            {
                                if (PushException(new CallException(er.Message, (int)ErrorTag.CallErr, er)))
                                    Log.Error($"CMD:{call.CmdTag} Error:\r\n{er}");

                            }

                        }
                        break;
                    case CmdDef.ReturnResult:
                        {


                            Result result = new Result
                            {
                                Id = read.ReadInt64(),
                                ErrorId = read.ReadInt32()
                            };
                            byte[] strdata = read.ReadByteArray();
                            if (strdata.Length > 0)
                                result.ErrorMsg = Encoding.UTF8.GetString(strdata);
                            var arglengt = read.ReadInt32();                        
                            for (int i = 0; i < arglengt; i++)                            
                                result.Arguments.Add(read.ReadByteArray());                            

                            SetReturnValue(result);
                            

                        }
                        break;
                    case CmdDef.SetSession:
                        {
                            if (read.ReadInt64(out long sessionid))
                            {
                                ClientManager.SessionRW.SetSession(sessionid);
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
                        call.SetRes(result);
                    }
                    catch (Exception er)
                    {
                        if(PushException(new SetResultException(er.Message, (int)ErrorTag.SetErr, er)))
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
                        if(PushException(new SetResultException(er.Message, (int)ErrorTag.SetErr, er)))
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
                IAsyncMethodDef method = Module.ModuleDiy[pack.CmdTag];


                object[] args = null;
                
                int argcount = 0;

                if (pack.Arguments != null)
                    argcount = pack.Arguments.Count;

                if (!method.IsNotRefAsyncArg)
                {

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
                        if (!method.IsRet)
                        {

                            AsyncCalls _calls_ = new AsyncCalls(this.LoggerFactory, pack.Id, pack.CmdTag, this, method.Obj, method.MethodInfo, args, false);
                            args[0] = _calls_;
                            _calls_.CallSend += SendData;
                            _calls_.ExceptionOut = this.ExceptionOut;
                            _calls_.Run();

                            AsyncCallDiy.AddOrUpdate(pack.Id, _calls_, (a, b) => _calls_);

                        }
                        else
                        {

                            AsyncCalls _calls_ = new AsyncCalls(this.LoggerFactory, pack.Id, pack.CmdTag, this, method.Obj, method.MethodInfo, args, true);
                            args[0] = _calls_;
                            _calls_.CallSend += SendData;
                            _calls_.Complete += RetrunResultData;
                            _calls_.ExceptionOut = this.ExceptionOut;
                            _calls_.Run();


                            AsyncCallDiy.AddOrUpdate(pack.Id, _calls_, (a, b) => _calls_);
                        }

                    }
                    else //SYNC
                    {
                        if (!method.IsRet)
                        {
                            method.MethodInfo.Invoke(method.Obj, args);
                        }
                        else
                        {
                            try
                            {
                                object res = method.MethodInfo.Invoke(method.Obj, args);

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

                                RetrunResultData(GetExceptionResult(er, pack.Id));

                                if (PushException(er))
                                    Log.Error($"Cmd:{pack.CmdTag} ERROR:{er.ToString()}");
                            }
                        }

                    }
                }
                else
                {
                    if (method.ArgsType.Length > 0 && method.ArgsType.Length == argcount)
                    {
                        args = new object[method.ArgsType.Length];                       
                       
                        for (int i = 0; i < method.ArgsType.Length ; i++)                                       
                            args[i] = Serialization.UnpackSingleObject(method.ArgsType[i], pack.Arguments[i]);
                        
                    }

                    if (method.IsAsync)
                    {
                        if (!method.IsRet)
                        {

                            AsyncCalls _calls_ = new AsyncCalls(this.LoggerFactory, pack.Id, pack.CmdTag, this, method.Obj, method.MethodInfo, args, false);                         
                            _calls_.CallSend += SendData;
                            _calls_.ExceptionOut = this.ExceptionOut;
                            _calls_.Run();

                            AsyncCallDiy.AddOrUpdate(pack.Id, _calls_, (a, b) => _calls_);

                        }
                        else
                        {

                            AsyncCalls _calls_ = new AsyncCalls(this.LoggerFactory, pack.Id, pack.CmdTag, this, method.Obj, method.MethodInfo, args, true);                        
                            _calls_.CallSend += SendData;
                            _calls_.Complete += RetrunResultData;
                            _calls_.ExceptionOut = this.ExceptionOut;
                            _calls_.Run();


                            AsyncCallDiy.AddOrUpdate(pack.Id, _calls_, (a, b) => _calls_);
                        }

                    }
                    else //SYNC
                    {
                        if (!method.IsRet)
                        {
                            method.MethodInfo.Invoke(method.Obj, args);
                        }
                        else
                        {
                            try
                            {
                                object res = method.MethodInfo.Invoke(method.Obj, args);

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

                                RetrunResultData(GetExceptionResult(er, pack.Id));

                                if (PushException(er))
                                    Log.Error($"Cmd:{pack.CmdTag} ERROR:{er.ToString()}");
                            }
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
                   
                    bufflist.Write(result.Id);
                    bufflist.Write(result.ErrorId);
                    if (string.IsNullOrEmpty(result.ErrorMsg))
                        bufflist.Write(0);
                    else
                    {
                        byte[] strdata = Encoding.UTF8.GetBytes(result.ErrorMsg);
                        bufflist.Write(strdata.Length);
                        bufflist.Write(strdata);
                    }

                    bufflist.Write(result.Arguments.Count);
                    foreach (var arg in result.Arguments)
                    {
                        bufflist.Write(arg.Length);
                        bufflist.Write(arg);
                    }

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
                   
                    bufflist.Write(result.Id);
                    bufflist.Write(result.ErrorId);
                    if (string.IsNullOrEmpty(result.ErrorMsg))
                        bufflist.Write(0);
                    else
                    {
                        byte[] strdata = Encoding.UTF8.GetBytes(result.ErrorMsg);
                        bufflist.Write(strdata.Length);
                        bufflist.Write(strdata);
                    }

                    bufflist.Write(result.Arguments.Count);
                    foreach (var arg in result.Arguments)
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
             

                SendData(pdata);

            }

        }

        public Result Res(params object[] args)
        {
            return new Result(args);
        }

        public Task<Result> ResTask(params object[] args)
        {       
            return Task.FromResult(new Result(args));
        }


        public void Close()
        {
            IsClose = true;
            AsyncWaitTimeOut.Clear();
            ClientManager.Close();
            Module.ModuleDiy.Clear();
            AsyncRunDiy.Clear();
            CallBackDiy.Clear();
            AsyncCallDiy.Clear();
            SyncWaitDic.Clear();
            FodyDir.Clear();
            Container.Dispose();
        }


        public void Dispose()
        {
            this.Close();
        }

      
    }
}
