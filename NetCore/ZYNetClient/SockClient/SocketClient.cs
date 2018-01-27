/*
 * 北风之神SOCKET框架(ZYSocket)
 *  Borey Socket Frame(ZYSocket)
 *  by luyikk@126.com
 *  Updated 2010-12-26 
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using ZYNet.CloudSystem.Client;
using System.Threading;

namespace ZYNet.CloudSystem.SocketClient
{

    public delegate void ConnectionOk(string message,bool IsConn);
    public delegate void DataOn(byte[] Data);
    public delegate void ExceptionDisconnection(string message);

    /// <summary>
    /// ZYSOCKET 客户端
    /// （一个简单的异步SOCKET客户端，性能不错。支持.NET 3.0以上版本。适用于silverlight)
    /// </summary>
    public class SocketClient:ISyncClient
    {
        /// <summary>
        /// SOCKET对象
        /// </summary>
        public Socket _sock { get; private set; }

        /// <summary>
        /// 连接成功事件
        /// </summary>
        public event ConnectionOk Connection;

        /// <summary>
        /// 数据包进入事件
        /// </summary>
        public event Action<byte[]> BinaryInput;
        /// <summary>
        /// 出错或断开触发事件
        /// </summary>
        public event Action<string> Disconnect;

        private System.Threading.AutoResetEvent wait = new System.Threading.AutoResetEvent(false);

        private AsyncSend _SendObj;
        
        public SocketClient()
        {           
            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _SendObj = new AsyncSend(_sock);
        }

        private bool IsConn;
        private int isR = 0;
       

        public SocketAsyncEventArgs AsynEvent { get; private set; }

        /// <summary>
        /// 异步连接到指定的服务器
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void BeginConnectionTo(string host, int port)
        {
            IPEndPoint myEnd = null;

            #region ipformat
            try
            {
                myEnd = new IPEndPoint(IPAddress.Parse(host), port);
            }
            catch (FormatException)
            {
#if!COREFX
                IPHostEntry p = Dns.GetHostEntry(Dns.GetHostName());
#else
                IPHostEntry p = Dns.GetHostEntryAsync(Dns.GetHostName()).Result;
#endif

                foreach (IPAddress s in p.AddressList)
                {
                    if (!s.IsIPv6LinkLocal)
                        myEnd = new IPEndPoint(s, port);
                }
            }

            #endregion

            SocketAsyncEventArgs e = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = myEnd
            };
            e.Completed += new EventHandler<SocketAsyncEventArgs>(Completed);
            AsynEvent = e;


            if (!_sock.ConnectAsync(e))
            {
                ECompleted(e);
            }
        }

        public bool Connect(string host, int port)
        {
            IPEndPoint myEnd = null;

            #region ipformat
            try
            {
                myEnd = new IPEndPoint(IPAddress.Parse(host), port);
            }
            catch (FormatException)
            {
#if !COREFX
                IPHostEntry p = Dns.GetHostEntry(Dns.GetHostName());
#else
                IPHostEntry p = Dns.GetHostEntryAsync(Dns.GetHostName()).Result;
#endif
                foreach (IPAddress s in p.AddressList)
                {
                    if (!s.IsIPv6LinkLocal)
                        myEnd = new IPEndPoint(s, port);
                }
            }

            #endregion

            SocketAsyncEventArgs e = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = myEnd               
            };

            e.Completed += new EventHandler<SocketAsyncEventArgs>(Completed);
            AsynEvent = e;
            if (!_sock.ConnectAsync(e))
            {
                ECompleted(e);
            }

            wait.WaitOne();
            wait.Reset();

            return IsConn;
        }



        void Completed(object sender, SocketAsyncEventArgs e)
        {
            ECompleted(e);
        }


        void ECompleted(SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:

                    if (e.SocketError == SocketError.Success)
                    {
                        Interlocked.Exchange(ref isR, 0);
                        IsConn = true;
                        wait.Set();

                        Connection?.Invoke("连接成功", true);

                        byte[] data = new byte[4096];
                        e.SetBuffer(data, 0, data.Length);  //设置数据包

                        try
                        {
                            if (!_sock.ReceiveAsync(e)) //开始读取数据包
                                ECompleted(e);
                        }
                        catch (ObjectDisposedException)
                        {
                            Disconnect?.Invoke("与服务器断开连接");
                        }

                    }
                    else
                    {
                        IsConn = false;
                        wait.Set();
                        Connection?.Invoke("连接失败", false);
                    }
                    break;

                case SocketAsyncOperation.Receive:
                    if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                    {
                        Interlocked.Exchange(ref isR, 0);

                        byte[] data = new byte[e.BytesTransferred];
                        Buffer.BlockCopy(e.Buffer, 0, data, 0, data.Length);

                      
                        //byte[] dataLast = new byte[4098];
                        //e.SetBuffer(dataLast, 0, dataLast.Length);

                        BinaryInput?.Invoke(data);

                        try
                        {

                            if (!_sock.ReceiveAsync(e))
                                ECompleted(e);

                        }
                        catch (ObjectDisposedException)
                        {
                            Disconnect?.Invoke("与服务器断开连接");
                        }

                    }
                    else
                    {
                        Disconnect?.Invoke("与服务器断开连接");
                    }
                    break;

            }
        }


     
        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="data"></param>
        public virtual void SendTo(byte[] data)
        {
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.SetBuffer(data, 0, data.Length);
            _sock.SendAsync(e);
        }

        public virtual void Send(byte[] data)
        {
            try
            {
                _SendObj.Send(data);

            }
            catch (ObjectDisposedException)
            {
                Disconnect?.Invoke("与服务器断开连接");
            }
            catch (SocketException)
            {
                try
                {
                    wait.Dispose();
                    AsynEvent.Dispose();

#if !COREFX
                    _sock.Close();
#endif
                    _sock.Dispose();
                 
                }
                catch { }

                Disconnect?.Invoke("与服务器断开连接");
            }          

            
        }



        public void Close()
        {
            try
            {
                _sock.Shutdown(SocketShutdown.Both);

#if !COREFX
                _sock.Disconnect(false);
                _sock.Close();
                if(wait!=null)
                    wait.Close();
#endif
                _sock.Dispose();
                if (wait != null)
                    wait.Dispose();

                AsynEvent.Dispose();

            }
            catch (ObjectDisposedException)
            {
            }
            catch (NullReferenceException)
            {

            }
        }
    }


    public interface ISend
    {
        bool Send(byte[] data);
    }
}
