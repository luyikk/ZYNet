using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.Loggine;
using ZYSocket.Server;
using ZYSocket.share;
using System.Linq;

namespace ZYNet.CloudSystem.Server
{
    public delegate bool IsCanConnHandler(IPEndPoint ipaddress);
    public class CloudServer
    {
        protected static readonly ILog Log = LogFactory.ForContext<CloudServer>();

        public ZYSocketSuper Server { get; private set; }

        public Func<Exception,bool> ExceptionOut { get; set; }

        Random Ran = new Random();

        int readOutTime;
        /// <summary>
        /// 设置超时时间
        /// </summary>
        public int ReadOutTime
        {
            get
            {
                return readOutTime;
            }
            set
            {
                readOutTime = value;
            }
        }
        
        public bool CheckTimeOut { get; set; } = false;

        public int MaxBuffsize { get; set; }

        public TimeSpan TokenWaitClecr { get; set; }


        /// <summary>
        /// 此IP是否可以连接?
        /// </summary>
        public event IsCanConnHandler IsCanConn;
        
        public Dictionary<int, AsyncMethodDef> CallsMethods { get; private set; }

        public ConcurrentDictionary<long,ASyncToken> TokenList { get; private set; }

        public Func<byte[],byte[]> DecodeingHandler { get; set; }

        public Func<byte[], byte[]> EcodeingHandler { get; set; }


        public CloudServer(string host, int port, int maxConnectCout, int maxBuffersize, int maxPackSize,int tokenWaitClecrMilliseconds = 30000)
        {
            TokenWaitClecr = TimeSpan.FromMilliseconds(tokenWaitClecrMilliseconds);
            Server = new ZYSocketSuper(host, port, maxConnectCout, maxBuffersize);
            MaxBuffsize = maxPackSize;
            Init();
        }

        private void Init()
        {
            
            CallsMethods = new Dictionary<int, AsyncMethodDef>();
            TokenList = new ConcurrentDictionary<long, ASyncToken>();
            Server.BinaryOffsetInput = BinaryInputOffsetHandler;
            Server.Connetions = ConnectionFilter;
            Server.MessageInput = MessageInputHandler;
            Server.IsOffsetInput = true;
            ReadOutTime = 5000;
            Task.Run(new Action(checkAsyncTimeOut));
        }

        private async void checkAsyncTimeOut()
        {
            while (true)
            {
                int timeSleep = 1;

                try
                {
                    bool c1 = checkTokenTimeOut();
                    bool c2 = checkAsynTokenTimeOut();           

                    if (c1 && c2)
                        timeSleep = 1000;
                    else if (c1)
                        timeSleep = 500;
                    else if (c2)
                        timeSleep = 500;
                    else
                        timeSleep = 20;

                }
                catch (Exception er)
                {
                    var b = ExceptionOut?.Invoke(er);
                    if(b is null)
                        Log.Error($"ERROR:\r\n{er.ToString()}");
                    else if(b.Value)                    
                        Log.Error($"ERROR:\r\n{er.ToString()}");
                    
                }
                finally
                {
                    await Task.Delay(timeSleep);
                }
            }
        }


        private bool checkTokenTimeOut()
        {
            bool isWaitlong = true;
            if (CheckTimeOut)            
                foreach (var token in TokenList)               
                    if(token.Value.CheckTimeOut())                   
                        isWaitlong = false;             

            return isWaitlong;
        }

        private bool checkAsynTokenTimeOut()
        {
            bool isWaitlong = true;
            var dis = TokenList.Values.Where(p => p.IsDisconnect);

            foreach (var token in dis)
                if ((DateTime.Now - TokenWaitClecr) > token.DisconnectDateTime)                
                    if (TokenList.TryRemove(token.SessionKey, out ASyncToken value))
                        Log.Debug($"Remove Token {value.SessionKey}");                

            return isWaitlong;
        }

        public CloudServer Install(Type packHandlerType)
        {

            if (packHandlerType.BaseType != typeof(ControllerBase))
                throw new TypeLoadException($"{packHandlerType.Name} not inherit ControllerBase");

            var methods = packHandlerType.GetMethods();

            Type tasktype = typeof(Task);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttributes(typeof(TAG), true);


                foreach (var att in attr)
                {
                    if (att is TAG attrcmdtype)
                    {

                        if (!CallsMethods.ContainsKey(attrcmdtype.CmdTag))
                        {
                            AsyncMethodDef tmp = new AsyncMethodDef(packHandlerType, method);
                            CallsMethods.Add(attrcmdtype.CmdTag, tmp);
                        }

                        break;
                    }

                }

            }

            return this;
        }
       

        public CloudServer Start()
        {
            Server.Start();
            Log.Info("Server is Start");
            return this;
        }

        public CloudServer Pause()
        {
            Server.Stop();
            Log.Info("Server is Pause");
            return this;
        }


        private bool ConnectionFilter(SocketAsyncEventArgs socketAsync)
        {

            Log.Trace(socketAsync.AcceptSocket.RemoteEndPoint + " Connect");

            return IsCanConn == null || IsCanConn((IPEndPoint)socketAsync.AcceptSocket.RemoteEndPoint);
        }


        /// <summary>
        /// 创建注册 ASyncToken
        /// </summary>
        /// <param name="socketAsync"></param>
        /// <returns></returns>
        private ASyncToken NewASyncToken(SocketAsyncEventArgs socketAsync, ZYNetRingBufferPool stream,long sessionkey)
        {
            ASyncToken tmp = new ASyncToken(socketAsync, this, sessionkey, stream);
            tmp.ExceptionOut = this.ExceptionOut;
            return tmp;
        }



        private void BinaryInputOffsetHandler(byte[] data, int offset, int count, SocketAsyncEventArgs socketAsync)
        {
            try
            {
                if (socketAsync.UserToken is ASyncToken tmp)
                    tmp.Write(data, offset, count);
                else 
                {                    
                    ZYNetRingBufferPool stream;
                    if (socketAsync.UserToken != null)
                        stream = socketAsync.UserToken as ZYNetRingBufferPool;
                    else
                    {
                        stream = new ZYNetRingBufferPool(MaxBuffsize);
                        socketAsync.UserToken = stream;
                    }

                    stream.Write(data, offset, count);

                    if (stream.Read(out byte[] pdata))
                    {
                        DataOn(pdata, socketAsync,stream);                     
                    }
                }
               
            }
            catch (Exception er)
            {
               var b=  ExceptionOut?.Invoke(er);
                if(b is null)
                    Log.Error(er.Message,er);
                else if(b.Value)
                    Log.Error(er.Message, er);

            }

        }

        private void DataOn(byte[] data,SocketAsyncEventArgs socketAsync, ZYNetRingBufferPool stream)
        {
            ReadBytes read = new ReadBytes(data);

            if(read.Length>=4)
            {
                int lengt;               

                if (read.ReadInt32(out lengt) && lengt == read.Length)
                {                    

                    if (read.ReadByte() == 0xED  &&
                       read.ReadByte() == 0xCE &&
                       read.ReadByte() == 0xFE &&
                       read.ReadByte() == 0x10)
                    {

                        long sessionId = read.ReadInt64();

                        if (sessionId == 0)
                        {
                            var token = MakeNewToken(socketAsync, stream, ref sessionId);
                            if (token != null)
                            {
                                BufferFormat session = new BufferFormat(0x10FECEED);
                                session.AddItem(sessionId);
                                Send(token.Sendobj, session.Finish());
                            }
                        }
                        else
                        {
                            if(TokenList.TryGetValue(sessionId,out ASyncToken token))
                            {

                                token.SetSocketEventAsync(socketAsync);
                                socketAsync.UserToken = token;
                                Log.Debug($"ReUse Token {token.SessionKey}");
                            }
                            else
                            {
                                var _token = MakeNewToken(socketAsync, stream, ref sessionId);
                                if (_token != null)
                                {
                                    BufferFormat session = new BufferFormat(0x10FECEED);
                                    session.AddItem(sessionId);
                                    Send(_token.Sendobj, session.Finish());
                                }
                            }
                        }
                    }
                    else
                        Server.Disconnect(socketAsync.AcceptSocket);
                }
                else
                    Server.Disconnect(socketAsync.AcceptSocket);
            }
            else            
                Server.Disconnect(socketAsync.AcceptSocket);

           
        }

        private ASyncToken MakeNewToken(SocketAsyncEventArgs socketAsync, ZYNetRingBufferPool stream,ref long sessionId)
        {
            sessionId = MakeSessionId();
            var token = NewASyncToken(socketAsync, stream, sessionId);
            socketAsync.UserToken = token;
            if (!TokenList.TryAdd(sessionId, token))
            {
                Server.Disconnect(socketAsync.AcceptSocket);
                return null;
            }

            Log.Debug($"Create Token {token.SessionKey}");

            return token;
        }


        private void MessageInputHandler(string message, SocketAsyncEventArgs socketAsync, int erorr)
        {
            if (socketAsync.UserToken != null)            
                if (socketAsync.UserToken is ASyncToken tmp)                
                    tmp.Disconnect(message);                
            

            socketAsync.UserToken = null;
            socketAsync.AcceptSocket.Close();
            socketAsync.AcceptSocket.Dispose();

            Log.Trace(message);
        }


        internal void Send(ISend sock, byte[] data)
        {
            Server.Send(sock, data);
        }
       
        internal long MakeSessionId()
        {
            lock (Ran)
            {
                long c = 630822816000000000; //2000-1-1 0:0:0:0
                long x = DateTime.Now.Ticks;
                long m = ((x - c) * 1000) + Ran.Next(1000);
                return m;
            }
        }
    }
}
