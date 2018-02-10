using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.Loggine;
using ZYSocket.Server;

namespace ZYNet.CloudSystem.Server
{
    public delegate bool IsCanConnHandler(IPEndPoint ipaddress);
    public class CloudServer
    {
        protected static readonly ILog Log = LogFactory.ForContext<CloudServer>();

        public ZYSocketSuper Server { get; private set; }


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
                if (value > 30000)
                    readOutTime = 30000;
                else
                    readOutTime = value;
            }
        }
        
        public bool CheckTimeOut { get; set; } = false;


        public int MaxBuffsize { get; set; }


        /// <summary>
        /// 此IP是否可以连接?
        /// </summary>
        public event IsCanConnHandler IsCanConn;

        
        public Dictionary<int, AsyncStaticMethodDef> CallsMethods { get; private set; }



        public Func<byte[],byte[]> DecodeingHandler { get; set; }

        public Func<byte[], byte[]> EcodeingHandler { get; set; }

 #if !COREFX
        public CloudServer()
        {
            Server = new ZYSocketSuper();
            MaxBuffsize = 1024 * 1024 * 2; //2M
            Init();
        }

        public CloudServer(int maxPackSize)
        {
            Server = new ZYSocketSuper();
            MaxBuffsize = maxPackSize;
            Init();
        }
#endif

        public CloudServer(string host, int port, int maxConnectCout, int maxBuffersize, int maxPackSize)
        {
            Server = new ZYSocketSuper(host, port, maxConnectCout, maxBuffersize);
            MaxBuffsize = maxPackSize;
            Init();
        }

        private void Init()
        {
            CallsMethods = new Dictionary<int, AsyncStaticMethodDef>();
            Server.BinaryOffsetInput = BinaryInputOffsetHandler;
            Server.Connetions = ConnectionFilter;
            Server.MessageInput = MessageInputHandler;
            Server.IsOffsetInput = true;
            ReadOutTime = 5000;
        }

        
        public CloudServer Install(Type packHandlerType)
        {
          

            var methods = packHandlerType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            Type tasktype = typeof(Task);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttributes(typeof(TAG), true);


                foreach (var att in attr)
                {
                    if (att is TAG attrcmdtype)
                    {

                        if ((method.ReturnType == tasktype || (Common.IsTypeOfBaseTypeIs(method.ReturnType, tasktype) && method.ReturnType.IsConstructedGenericType && method.ReturnType.GenericTypeArguments[0] == typeof(Result))))
                        {
                            if (method.GetParameters().Length > 0 && method.GetParameters()[0].ParameterType == typeof(AsyncCalls))
                            {
                                if (!CallsMethods.ContainsKey(attrcmdtype.CmdTag))
                                {
                                    AsyncStaticMethodDef tmp = new AsyncStaticMethodDef(method);
                                    CallsMethods.Add(attrcmdtype.CmdTag, tmp);
                                }
                            }

                        }

                        else if (method.GetParameters().Length > 0 && method.GetParameters()[0].ParameterType == typeof(ASyncToken))
                        {
                            if (!CallsMethods.ContainsKey(attrcmdtype.CmdTag))
                            {
                                AsyncStaticMethodDef tmp = new AsyncStaticMethodDef(method);
                                CallsMethods.Add(attrcmdtype.CmdTag, tmp);
                            }
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
        private ASyncToken NewASyncToken(SocketAsyncEventArgs socketAsync)
        {
            ASyncToken tmp = new ASyncToken(socketAsync, this, MaxBuffsize);

            return tmp;
        }



        private void BinaryInputOffsetHandler(byte[] data, int offset, int count, SocketAsyncEventArgs socketAsync)
        {
            try
            {              
                if (socketAsync.UserToken is ASyncToken tmp)
                    tmp.Write(data, offset, count);
                else if (count >= 8 && data[offset] == 0xFF && data[offset + 1] == 0xFE && data[offset + 5] == 0xCE &&
                         data[offset + 7] == 0xED)
                {
                    var token = NewASyncToken(socketAsync);
                    socketAsync.UserToken = token;

                    if (count > 8)
                    {
                        byte[] bakdata = new byte[count - 8];
                        Buffer.BlockCopy(data, offset + 8, bakdata, 0, bakdata.Length);
                        token.Write(bakdata, 0, bakdata.Length);
                    }
                }
                else
                {
                    Server.Disconnect(socketAsync.AcceptSocket);

                }
            }
            catch (Exception er)
            {
                Log.Error(er.Message,er);
            }

        }


        private void MessageInputHandler(string message, SocketAsyncEventArgs socketAsync, int erorr)
        {
            if (socketAsync.UserToken != null)
            {             
                if (socketAsync.UserToken is ASyncToken tmp)
                    tmp.Disconnect(message);

            }

            socketAsync.UserToken = null;
#if !COREFX
            socketAsync.AcceptSocket.Close();
#endif
            socketAsync.AcceptSocket.Dispose();

            Log.Trace(message);
        }


        internal void Send(ISend sock, byte[] data)
        {
            Server.Send(sock, data);
        }
       

    }
}
