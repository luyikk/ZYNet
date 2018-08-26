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
using System.Reflection;

namespace ZYNet.CloudSystem.Server
{
    public class ASyncToken : ZYSync, IASync
    {
        protected static readonly ILog Log = LogFactory.ForContext<ASyncToken>();


        private Dictionary<Type, Type> FodyDir { get; set; }

        public SocketAsyncEventArgs Asyn { get; private set; }
        public ZYNetRingBufferPool Stream { get; private set; }

        public CloudServer CurrentServer { get;  private set; }
        public List<KeyValuePair<long, DateTime>> AsyncWaitTimeOut { get; private set; }
        public ConcurrentDictionary<long, AsyncCalls> AsyncCallDict { get; private set; }
        public ConcurrentDictionary<long, AsyncCalls> CallBackDict { get; private set; }
        public ConcurrentDictionary<Type, ControllerBase> ControllerDict { get; private set; }

        public Action<ASyncToken,string> UserDisconnect { get; set; }

        public AsyncSend Sendobj { get; private set; }

        public object UserToken { get; set; }

        public bool IsValidate { get; set; }

        public long SessionKey { get; private set; }
        public bool IsDisconnect { get; private set; }
        public DateTime DisconnectDateTime { get; private set; }


        public ASyncToken(SocketAsyncEventArgs asynca, CloudServer server,long sessionKey, int MaxBuffsize)
        {
            this.Asyn = asynca;
            Stream = new ZYNetRingBufferPool(MaxBuffsize);
            CurrentServer = server;
            this.DataExtra = server.EcodeingHandler;
            Sendobj = new AsyncSend(asynca.AcceptSocket);
            AsyncWaitTimeOut = new List<KeyValuePair<long, DateTime>>();
            FodyDir = new Dictionary<Type, Type>();
            SessionKey = SessionKey;
        }

        public ASyncToken(SocketAsyncEventArgs asynca, CloudServer server,long sessionKey, ZYNetRingBufferPool stream)
        {
            this.Asyn = asynca;
            Stream = stream;
            CurrentServer = server;
            this.DataExtra = server.EcodeingHandler;
            Sendobj = new AsyncSend(asynca.AcceptSocket);
            AsyncWaitTimeOut = new List<KeyValuePair<long, DateTime>>();
            FodyDir = new Dictionary<Type, Type>();
            SessionKey = sessionKey; 
        }

     


        public void SetSocketEventAsync(SocketAsyncEventArgs asynca)
        {
            this.Asyn = asynca;
            this.Sendobj = new AsyncSend(asynca.AcceptSocket);
            IsDisconnect = false;
        }


        public T Token<T>()
        {
            if (UserToken == null)
                return default(T);
            else
                return (T)UserToken;
        }


        public Result Res(params object[] args)
        {
            return new Result(args);          
        }

        public Task<Result> ResTask(params object[] args)
        {
            return Task.FromResult(new Result(args));
        }


        public AsyncCalls MakeAsync(AsyncCalls async)
        {
            AsyncCalls tmp = new AsyncCalls(this, async._fiber);
            tmp.CallSend += SendData;
            tmp.ExceptionOut = this.ExceptionOut;
            return tmp;
        }


        public void Disconnect()
        {
            var server = CurrentServer?.Server;
            var socket = Asyn?.AcceptSocket;
            if (server != null && socket != null)
                server.Disconnect(Asyn.AcceptSocket);
        }

        public ASyncToken GetAsyncToken()
        {
            return this;
        }



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
                    throw new FodyInstallException("not find with {interfaceType.FullName} the Implementation check FodyWeavers.xml and nuget fody",(int)ErrorTag.FodyInstallErr);
                FodyDir.Add(interfaceType, implementationType);
                return (T)Activator.CreateInstance(implementationType, new Func<int, Type, object[], object>(Call));

            }
            else
            {
                return (T)Activator.CreateInstance(FodyDir[interfaceType], new Func<int, Type, object[], object>(Call));

            }

        }

        protected virtual object Call(int cmd, Type returnType, object[] args)
        {

            if (returnType != typeof(void))
            {

                if (!Common.IsTypeOfBaseTypeIs(returnType, typeof(FiberThreadAwaiterBase)))
                {
                    if (returnType == typeof(Result))
                    {
                        return Func(cmd, args);
                    }
                    else
                    {
                        try
                        {
                            return Func(cmd, args)?.First?.Value(returnType);
                        }
                        catch (Exception er)
                        {
                            throw new ReturnTypeException(string.Format("Return Type of {0} Error", returnType),(int)ErrorTag.ReturnTypeErr, er);
                        }
                    }
                }
                else
                {
                    throw new ReturnTypeException("Sync Call Not Use Return Type Of ResultAwatier", (int)ErrorTag.ReturnTypeErr);
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
                        if (method.ReturnType == typeof(Result))
                        {
                            return Func(cmd, args);
                        }
                        else
                        {
                            try
                            {
                                return Func(cmd, args)?.First?.Value(method.ReturnType);
                            }
                            catch (Exception er)
                            {
                                throw new ReturnTypeException(string.Format("Return Type of {0} Error", method.ReturnType), (int)ErrorTag.ReturnTypeErr, er);
                            }
                        }
                    }
                    else
                    {
                        throw new ReturnTypeException("Sync Call Not Use Return Type Of ResultAwatier", (int)ErrorTag.ReturnTypeErr);
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

#else
         public T GetForEmit<T>()
         {
            return Get<T>();
         }

#endif





        internal void RemoveAsyncCall(long id)
        {           
            if(AsyncCallDict.ContainsKey(id))
            if(!AsyncCallDict.TryRemove(id, out AsyncCalls call))
            {
                Log.Error($"Id:{id} ERROR:\r\nNot TryRemove AsyncCall");
            }          
         
        }

        internal bool CheckTimeOut()
        {
            if (CallBackDict == null || CallBackDict.Count == 0)
                return false;
            else
            {
                var res = AsyncWaitTimeOut.FindAll(p => p.Value < DateTime.Now);

                if (res.Count == 0)
                    return false;
                else
                {
                    foreach (var item in res)
                    {
                        long id = item.Key;
                        if (CallBackDict.ContainsKey(id))
                        {
                            if (CallBackDict.TryRemove(id, out AsyncCalls value))
                            {
                                Task.Run(() =>
                                {

                                    var timeout = GetExceptionResult("Server call client time out", (int)ErrorTag.TimeOut, id);

                                    try
                                    {
                                        value.SetRes(timeout);
                                    }
                                    catch (Exception er)
                                    {
                                        if(PushException(new SetResultException(er.Message, (int)ErrorTag.SetErr, er)))
                                            Log.Error($"CMD:{value.Cmd} Id:{value.Id} ERROR:\r\n{er.Message}");
                                    }

                                });
                            }
                        }

                        AsyncWaitTimeOut.Remove(item);
                    }

                    return true;
                }
            }
        }

        internal  void AddAsyncCallBack(AsyncCalls asyncalls, long id)
        {
            if (CallBackDict == null)
                CallBackDict = new ConcurrentDictionary<long, AsyncCalls>();

            CallBackDict.AddOrUpdate(id, asyncalls, (a, b) => asyncalls);

            if (asyncalls.CurrentServer.CheckTimeOut)
            {
                KeyValuePair<long, DateTime> tot = new KeyValuePair<long, DateTime>(id, DateTime.Now.AddMilliseconds(asyncalls.CurrentServer.ReadOutTime));
                AsyncWaitTimeOut.Add(tot);
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
           
            this.Asyn = null;           
            Stream.Flush();
            IsDisconnect = true;
            DisconnectDateTime = DateTime.Now;

            if (AsyncCallDict!=null)
                AsyncCallDict.Clear();

            if (CallBackDict != null)
            {

                foreach (var item in CallBackDict.Values)
                {                   
                    var disconn = GetExceptionResult("Disconnect", (int)ErrorTag.Disconnect, item.Id);
                    disconn.Arguments = new List<byte[]>();
                    item.SetRes(disconn);
                }


                CallBackDict.Clear();
            }        

            UserDisconnect?.Invoke(this, message);
            UserDisconnect = null;
        }

        private void DataOn(byte[] data)
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

                            Result result = new Result();
                            result.Id = read.ReadInt64();
                            result.ErrorId = read.ReadInt32();
                            byte[] strdata = read.ReadByteArray();
                            if (strdata.Length > 0)
                                result.ErrorMsg = Encoding.UTF8.GetString(strdata);                          
                            var arglengt = read.ReadInt32();                           
                            for (int i = 0; i < arglengt; i++)                            
                                result.Arguments.Add(read.ReadByteArray());

                            if (CallBackDict.ContainsKey(result.Id))
                            {

                                if (CallBackDict.TryRemove(result.Id, out AsyncCalls call))
                                {
                                    try
                                    {
                                        call.SetRes(result);

                                    }
                                    catch (Exception er)
                                    {
                                        var ermsg = $"Cmd:{call.Cmd} Error:\r\n {er}";

                                        if (PushException(new CallException(ermsg, (int)ErrorTag.CallErr, er)))
                                            Log.Error(ermsg);
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
            throw new InvalidOperationException("Server Sync Func Not Use!! please use Action");
        }

        private void ResrunResultData(Result result)
        {
            using (MemoryStream stream = new MemoryStream())
            {

                BinaryWriter bufflist = new BinaryWriter(stream);

                if (DataExtra != null)
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

        private  void CallPackRun(CallPack pack)
        {
            if (CurrentServer.CallsMethods.ContainsKey(pack.CmdTag))
            {
                if (AsyncCallDict == null)
                    AsyncCallDict = new ConcurrentDictionary<long, AsyncCalls>();
                if (ControllerDict == null)
                    ControllerDict = new ConcurrentDictionary<Type, ControllerBase>();

                var method = CurrentServer.CallsMethods[pack.CmdTag];

                ControllerBase implement;

                if (!ControllerDict.TryGetValue(method.ImplementationType, out implement))
                {
                    implement = (ControllerBase)Activator.CreateInstance(method.ImplementationType,this);
                    ControllerDict[method.ImplementationType] = implement;
                }
                


                object[] args = null;

                int argcount = 0;

                if (pack.Arguments != null)
                    argcount = pack.Arguments.Count;

                if (method.ArgsType.Length > 0 && method.ArgsType.Length == argcount)
                {
                    args = new object[method.ArgsType.Length];
                  
                    for (int i = 0; i < method.ArgsType.Length; i++)
                    {                      
                        args[i] = Serialization.UnpackSingleObject(method.ArgsType[i], pack.Arguments[i]);
                    }
                }
           
                if (method.IsAsync)
                {
                    if (!method.IsRet)
                    {

                        AsyncCalls _calls_ = new AsyncCalls(pack.Id, pack.CmdTag,  this, implement,method.MethodInfo, args, false);
                        implement.CurrentAsync = _calls_;
                        _calls_.CallSend += SendData;
                        _calls_.ExceptionOut = this.ExceptionOut;
                        AsyncCallDict.AddOrUpdate(pack.Id, _calls_, (a, b) => _calls_);
                        _calls_.Run();

                    }
                    else
                    {

                        AsyncCalls _calls_ = new AsyncCalls(pack.Id, pack.CmdTag, this, implement,method.MethodInfo, args, true);
                        implement.CurrentAsync = _calls_;
                        _calls_.CallSend += SendData;
                        _calls_.Complete += ResrunResultData;
                        _calls_.ExceptionOut = this.ExceptionOut;
                        AsyncCallDict.AddOrUpdate(pack.Id, _calls_, (a, b) => _calls_);
                        _calls_.Run();
                    }
                }
                else //SYNC
                {
                    if (!method.IsRet)
                    {
                        method.MethodInfo.Invoke(implement, args);
                    }
                    else
                    {
                        try
                        {
                            object res = method.MethodInfo.Invoke(implement, args);

                            if (res != null)
                            {
                                if (res is Result result)
                                {
                                    result.Id = pack.Id;

                                    ResrunResultData(result);
                                }
                                else
                                {
                                    Result tmp = new Result(res)
                                    {
                                        Id = pack.Id
                                    };

                                    ResrunResultData(tmp);
                                }
                            }
                        }
                        catch (Exception er)
                        {
                            var tmp = base.GetExceptionResult(er, pack.Id);
                            ResrunResultData(tmp);
                            if (PushException(er))
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
