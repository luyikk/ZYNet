using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Loggine;
using ZYSocket.Server;
using ZYSocket.share;


namespace ZYNet.CloudSystem.Server
{
    public delegate bool IsCanConnHandler(IPEndPoint ipaddress);
    public partial class CloudServer:InstallController
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

    


        private void MessageInputHandler(string message, SocketAsyncEventArgs socketAsync, int erorr)
        {
            try
            {
                if (socketAsync.UserToken != null)
                    if (socketAsync.UserToken is ASyncToken tmp)
                        tmp.Disconnect(message);


                socketAsync.UserToken = null;
                socketAsync.AcceptSocket.Close();
                socketAsync.AcceptSocket.Dispose();

                Log.Trace(message);
            }
            catch (Exception er)
            {
                var b = ExceptionOut?.Invoke(er);
                if (b is null)
                    Log.Error(er.Message, er);
                else if (b.Value)
                    Log.Error(er.Message, er);
            }
        }


        internal void Send(ISend sock, byte[] data)
        {
            Server.Send(sock, data);
        }

     
    }
}
