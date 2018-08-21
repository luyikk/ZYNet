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
    public class ASyncToken:ZYSync
    {
        protected static readonly ILog Log = LogFactory.ForContext<ASyncToken>();

        public SocketAsyncEventArgs Asyn { get; private set; }
        public ZYNetRingBufferPool Stream { get; private set; }

        public CloudServer CurrentServer { get;  private set; }
        public List<KeyValuePair<long, DateTime>> AsyncWaitTimeOut { get; private set; }
        public ConcurrentDictionary<long, AsyncCalls> AsyncCallDiy { get; private set; }
        public ConcurrentDictionary<long, AsyncCalls> CallBackDiy { get; private set; }

        private Dictionary<Type, Type> FodyDir { get; set; }

        public event Action<ASyncToken,string> UserDisconnect;

        public AsyncSend Sendobj { get; private set; }

        public object UserToken { get; set; }

        public bool IsValidate { get; set; }




        public ASyncToken(SocketAsyncEventArgs asynca, CloudServer server, int MaxBuffsize)
        {
            this.Asyn = asynca;
            Stream = new ZYNetRingBufferPool(MaxBuffsize);
            CurrentServer = server;
            this.DataExtra = server.EcodeingHandler;
            Sendobj = new AsyncSend(asynca.AcceptSocket);
            AsyncWaitTimeOut = new List<KeyValuePair<long, DateTime>>();
            FodyDir = new Dictionary<Type, Type>();
        }



        public T Token<T>()
        {
            if (UserToken == null)
                return default(T);
            else
                return (T)UserToken;
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
                    throw new Exception("not find with {interfaceType.FullName} the Implementation check FodyWeavers.xml and nuget fody");
                FodyDir.Add(interfaceType, implementationType);
                return (T)Activator.CreateInstance(implementationType, new Func<int, Type, object[], object>(Call));

            }
            else
            {
                return (T)Activator.CreateInstance(FodyDir[interfaceType], new Func<int, Type, object[], object>(Call));

            }

        }

        public virtual object Call(int cmd, Type returnType, object[] args)
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
                            throw new Exception(string.Format("Return Type of {0} Error", returnType), er);
                        }
                    }
                }
                else
                {
                    throw new Exception("Sync Call Not Use Return Type Of ResultAwatier");
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
                                throw new Exception(string.Format("Return Type of {0} Error", method.ReturnType), er);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Sync Call Not Use Return Type Of ResultAwatier");
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












        internal void RemoveAsyncCall(long id)
        {           
            if(AsyncCallDiy.ContainsKey(id))
            if(!AsyncCallDiy.TryRemove(id, out AsyncCalls call))
            {
                Log.Error($"Id:{id} ERROR:\r\nNot TryRemove AsyncCall");
            }          
         
        }

        internal bool CheckTimeOut()
        {
            if (CallBackDiy == null || CallBackDiy.Count == 0)
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
                        if (CallBackDiy.ContainsKey(id))
                        {
                            if (CallBackDiy.TryRemove(id, out AsyncCalls value))
                            {
                                Task.Run(() =>
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
            if (CallBackDiy == null)
                CallBackDiy = new ConcurrentDictionary<long, AsyncCalls>();

            CallBackDiy.AddOrUpdate(id, asyncalls, (a, b) => asyncalls);

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
                    if (!method.IsRet)
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
                    if (!method.IsRet)
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
