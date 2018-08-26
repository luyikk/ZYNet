using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Client;
using ZYSocket.share;

namespace ZYNet.CloudSystem.SocketClient
{
    public class ConnectionManager : IConnectionManager
    {
        private object lockObj = new object();
        /// <summary>
        /// 数据包进入事件
        /// </summary>
        public event Action<byte[]> BinaryInput;
        /// <summary>
        /// 出错或断开触发事件
        /// </summary>
        public event Action<string> Disconnect;

        private bool IsClose;
        private SocketClient socketClient;
        public ISyncClient Sock => socketClient;

        public ZYNetRingBufferPool RingBuffer { get; private set; }

        public string Host { get; private set; }

        public int Port { get; private set; }

        public int MaxBufferLength { get; private set; }

        public ISessionRW SessionRW { get; set; }

        public bool IsConnect
        {
            get
            {
                if (socketClient is null)
                    return false;
                else
                    return socketClient.IsConnect;
            }
        }

        public ConnectionManager(ISessionRW sessionRW=null)
        {
            if (sessionRW == null)
                SessionRW = new SessionRWMemory();
            else
                SessionRW = sessionRW;
        }

     
        public bool Install(string host,int port,int maxBufferLength)
        {
            this.Host = host;
            this.Port = port;
            this.MaxBufferLength = maxBufferLength;
            RingBuffer = new ZYNetRingBufferPool(MaxBufferLength);
            return CheckSockConnect();
        }

        public Task<bool> InstallAsync(string host, int port, int maxBufferLength)
        {
            this.Host = host;
            this.Port = port;
            this.MaxBufferLength = maxBufferLength;
            RingBuffer = new ZYNetRingBufferPool(MaxBufferLength);

            return  CheckSockConnectAsync();
        }

        public void Close()
        {
            if (socketClient != null)
                socketClient.Close();

            RingBuffer.Flush();
            RingBuffer = null;
            socketClient = null;
            IsClose = true;
        }


        private bool CheckSockConnect()
        {
            if (IsClose)
                throw new ObjectDisposedException("ConnectionManager is Close");

            lock (lockObj)
            {
                if (socketClient == null || socketClient.IsConnect == false)
                {
                    socketClient?.Close();
                    socketClient = new SocketClient();

                    if (socketClient.Connect(Host, Port))
                    {

                        RingBuffer.Flush();
                        socketClient.BinaryInput += SocketClient_BinaryInput; 
                        socketClient.Disconnect += SocketClient_Disconnect;                      

                        BufferFormat buffer = new BufferFormat(0x10FECEED);
                        buffer.AddItem(SessionRW.GetSession());
                        byte[] data = buffer.Finish();
                        socketClient.Send(data);
                     

                        return true;

                    }
                    else
                        return false;
                }
                else
                {
                    return true;
                }
            }
        }


        private async Task<bool> CheckSockConnectAsync()
        {
            return await Task.Run(() =>
            {
                return CheckSockConnect();
            });
        }


        private void SocketClient_Disconnect(string obj)
        {
            Disconnect?.Invoke(obj);
            
        }

        private void SocketClient_BinaryInput(byte[] data)
        {
            RingBuffer.Write(data);

            while (RingBuffer.Read(out byte[] pdata))            
                    BinaryInput?.Invoke(pdata);
            
        }

        public void SendData(byte[] data)
        {
            if (CheckSockConnect())
            {
                socketClient.Send(data);
            }
            else
                throw new TimeoutException("not connect server");
        }


    }
}
