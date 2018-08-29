using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ZYSocket.Server
{
    public class BeginSend : ISend //供性能对比测试
    {
        private Socket _sock { get; set; }

        protected int BufferLenght { get; set; } = -1;

        public BeginSend(Socket sock, int bufferLength)
        {
            this.BufferLenght = bufferLength;
            this._sock = sock;
        }


        public bool Send(byte[] data)
        {
            _sock.BeginSend(data, 0, data.Length, SocketFlags.None, AsynCallBack, _sock);
            return true;
        }

        void AsynCallBack(IAsyncResult result)
        {
            try
            {
                Socket sock = result.AsyncState as Socket;

                if (sock != null)
                {
                    sock.EndSend(result);
                }
            }
            catch
            {

            }
        }
    }
}
