using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Client;

namespace ZYNet.CloudSystem.SockClient
{
    public class SocketClient : ISyncClient
    {
        public event Action<byte[]> BinaryInput;
        public event Action<string> Disconnect;

    
        /// <summary>
        /// SOCKET对象
        /// </summary>
        private Socket sock;

        private bool IsConn;

        private System.Threading.AutoResetEvent wait = new System.Threading.AutoResetEvent(false);

        public SocketClient()
        {
           

            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
                IPHostEntry p = Dns.GetHostEntry(Dns.GetHostName());

                foreach (IPAddress s in p.AddressList)
                {
                    if (!s.IsIPv6LinkLocal)
                        myEnd = new IPEndPoint(s, port);
                }
            }

            #endregion

            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.RemoteEndPoint = myEnd;
            e.Completed += new EventHandler<SocketAsyncEventArgs>(e_Completed);
            if (!sock.ConnectAsync(e))
            {
                eCompleted(e);
            }

            wait.WaitOne();
            wait.Reset();

            return IsConn;
        }

        void e_Completed(object sender, SocketAsyncEventArgs e)
        {
            eCompleted(e);
        }


        void eCompleted(SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:

                    if (e.SocketError == SocketError.Success)
                    {

                        IsConn = true;
                        wait.Set();

                        byte[] data = new byte[4096];
                        e.SetBuffer(data, 0, data.Length);  //设置数据包

                        if (!sock.ReceiveAsync(e)) //开始读取数据包
                            eCompleted(e);

                    }
                    else
                    {
                        IsConn = false;
                        wait.Set();                       
                    }
                    break;

                case SocketAsyncOperation.Receive:
                    if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                    {
                        byte[] data = new byte[e.BytesTransferred];
                        Buffer.BlockCopy(e.Buffer, 0, data, 0, data.Length);

                        byte[] dataLast = new byte[4098];
                        e.SetBuffer(dataLast, 0, dataLast.Length);

                        if (BinaryInput != null)
                            BinaryInput(data);

                        if (!sock.ReceiveAsync(e))
                            eCompleted(e);

                      

                    }
                    else
                    {
                        if (Disconnect != null)
                            Disconnect("与服务器断开连接");
                    }
                    break;

            }
        }



        public void Send(byte[] data)
        {
            SocketAsyncEventArgs e =new SocketAsyncEventArgs();         

            if (e == null)
            {
                e = new SocketAsyncEventArgs();
            }

            e.Completed += E_Completed;
            e.SetBuffer(data, 0, data.Length);
            sock.SendAsync(e);
        }

        private void E_Completed(object sender, SocketAsyncEventArgs e)
        {
            e.AcceptSocket = null;          
        }

        public void Close()
        {
            try
            {
                sock.Shutdown(SocketShutdown.Both);
                sock.Disconnect(false);
                sock.Close();
                wait.Close();

            }
            catch (ObjectDisposedException)
            {
            }
            catch (NullReferenceException)
            {

            }
        }
    }
}
