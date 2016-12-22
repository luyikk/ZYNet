using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;
using ZYSocket.Server;
using ZYSocket.share;

namespace ZYNet.CloudSystem.Server
{
    public delegate bool IsCanConnHandler(IPEndPoint ipaddress);
    public class CloudServer
    {
        public ZYSocketSuper Server { get; private set; }

        /// <summary>
        /// 设置超时时间
        /// </summary>
        public int ReadOutTime
        {
            get; set;
        }



        public int MaxBuffsize { get; set; }


        /// <summary>
        /// 此IP是否可以连接?
        /// </summary>
        public event IsCanConnHandler IsCanConn;



        
        public Dictionary<int, AsyncStaticMethodDef> CallsMethods { get; private set; }



        public RDataExtraHandle DecodeingHandler { get; set; }

        public RDataExtraHandle EcodeingHandler { get; set; }


        public CloudServer()
        {
            Server = new ZYSocketSuper();
            MaxBuffsize = 1024 * 1024 * 2; //2M
            Init();
        }

        public CloudServer(string host, int port, int maxconnectcout, int maxbuffersize, int maxPackSize)
        {
            Server = new ZYSocketSuper(host, port, maxconnectcout, maxbuffersize);
            MaxBuffsize = maxPackSize;
            Init();
        }

        private void Init()
        {
            CallsMethods = new Dictionary<int, AsyncStaticMethodDef>();
            Server.BinaryOffsetInput = new BinaryInputOffsetHandler(BinaryInputOffsetHandler);
            Server.Connetions = new ConnectionFilter(ConnectionFilter);
            Server.MessageInput = new MessageInputHandler(MessageInputHandler);
            Server.IsOffsetInput = true;
            ReadOutTime = 2000;
        }

        
        public void Install(Type packHandlerType)
        {
          

            var methods = packHandlerType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            Type tasktype = typeof(Task);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttributes(typeof(MethodRun), true);

                if (attr.Length > 0)
                {
                    foreach (var att in attr)
                    {
                        MethodRun attrcmdtype = att as MethodRun;

                        if (attrcmdtype != null)
                        {
                            if ((method.ReturnType == tasktype || (method.ReturnType.BaseType == tasktype && method.ReturnType.IsConstructedGenericType && method.ReturnType.GenericTypeArguments[0] == typeof(ReturnResult))))
                            {
                                if (method.GetParameters().Length > 0 && method.GetParameters()[0].ParameterType == typeof(AsyncCalls))
                                {
                                    if (!CallsMethods.ContainsKey(attrcmdtype.CmdType))
                                    {
                                        AsyncStaticMethodDef tmp = new AsyncStaticMethodDef(method);
                                        CallsMethods.Add(attrcmdtype.CmdType, tmp);
                                    }
                                }

                            }
                            else if (method.ReturnType == null || method.ReturnType == typeof(void) || method.ReturnType != tasktype || method.ReturnType.BaseType != tasktype)
                            {

                                if (method.GetParameters().Length > 0 && method.GetParameters()[0].ParameterType == typeof(ASyncToken))
                                {
                                    if (!CallsMethods.ContainsKey(attrcmdtype.CmdType))
                                    {
                                        AsyncStaticMethodDef tmp = new AsyncStaticMethodDef(method);
                                        CallsMethods.Add(attrcmdtype.CmdType, tmp);
                                    }
                                }
                            }
                            break;
                        }

                    }
                }
            }
        }
       

        public void Start()
        {
            Server.Start();
            LogAction.Log("Server is Start");
        }

        public void Pause()
        {
            Server.Stop();
            LogAction.Log("Server is Pause");
        }


        private bool ConnectionFilter(SocketAsyncEventArgs socketNetCloudSystem)
        {

            LogAction.Log(socketNetCloudSystem.AcceptSocket.RemoteEndPoint.ToString() + " Connect");

            if (IsCanConn != null)
            {
                if (IsCanConn((IPEndPoint)socketNetCloudSystem.AcceptSocket.RemoteEndPoint))
                {
                    socketNetCloudSystem.UserToken = NewASyncToken(socketNetCloudSystem);
                    return true;
                }
                else
                    return false;
            }

            socketNetCloudSystem.UserToken = NewASyncToken(socketNetCloudSystem);

            return true;
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
            ASyncToken tmp = socketAsync.UserToken as ASyncToken;

            if (tmp != null)
                tmp.Write(data, offset, count);

        }


        private void MessageInputHandler(string message, SocketAsyncEventArgs socketAsync, int erorr)
        {
            if (socketAsync.UserToken != null)
            {
                ASyncToken tmp = socketAsync.UserToken as ASyncToken;
                if (tmp != null)
                    tmp.Disconnect(message);

            }

            socketAsync.UserToken = null;
            socketAsync.AcceptSocket.Close();
            socketAsync.AcceptSocket.Dispose();
            LogAction.Log(message);
        }


        internal void Send(Socket sock, byte[] data)
        {
            Server.SendData(sock, data);
        }
       

    }
}
