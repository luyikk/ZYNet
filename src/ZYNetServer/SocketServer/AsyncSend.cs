using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;


namespace ZYSocket.Server
{
    public class AsyncSend : ISend
    {
        private readonly int bufferlength = 4096;

        private SocketAsyncEventArgs _send { get; set; }

        private ConcurrentQueue<byte[]> BufferQueue { get; set; }

        private Socket _sock { get; set; }

        protected int BufferLenght { get; set; } = -1;

        private int SendIng;

        public AsyncSend(Socket sock, int bufferLength)
        {
            this._sock = sock;
            this.BufferLenght = bufferLength;
            SendIng = 0;
            BufferQueue = new ConcurrentQueue<byte[]>();
            _send = new SocketAsyncEventArgs();
            _send.Completed += Completed;
        }

        private void Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    {
                        BeginSend(e);
                    }
                    break;

            }
        }

        private void BeginSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Free();
                return;
            }


            Interlocked.Exchange(ref SendIng, 0);
            if (BufferQueue.Count > 0)
                SendComputer();


        }
    
        private void Free()
        {
            _send.BufferList = null;           
            for (int i = 0; i < BufferQueue.Count; i++)
                BufferQueue.TryDequeue(out byte[] tmp);
        }

        private bool InitData()
        {
            var list = PopBufferList();

            if(list.Count>0)
            {
                _send.BufferList = list;

                return true;
            }
            else
                return false;

        }



        public List<ArraySegment<byte>> PopBufferList()
        {
            List<ArraySegment<byte>> buffer = new List<ArraySegment<byte>>(BufferQueue.Count);

            int c = 0;
            while (true)
            {
                if (BufferQueue.TryDequeue(out byte[] data))
                {
                    if (BufferLenght <= 0)
                    {
                        ArraySegment<byte> vs = new ArraySegment<byte>(data, 0, data.Length);
                        buffer.Add(vs);
                    }
                    else
                    {
                        int length = 0;
                        int offset = 0;
                        do
                        {


                            if (BufferLenght > (data.Length - offset))
                                length = (data.Length - offset);
                            else
                                length = BufferLenght;

                            ArraySegment<byte> vs = new ArraySegment<byte>(data, offset, length);
                            buffer.Add(vs);
                            offset += length;

                        } while (offset < data.Length);

                    }

                    c += data.Length;

                    if (c > bufferlength)
                        break;
                   
                }
                else
                    break;
            }

            return buffer;
        }



        public bool Send(byte[] data)
        {

            if (_sock == null)
                return false;
            if (data == null)
                return false;

            BufferQueue.Enqueue(data);

            return SendComputer();

        }



        private bool SendComputer()
        {
            if (Interlocked.CompareExchange(ref SendIng, 1, 0) == 0)            
                if (InitData())
                {
                    SendAsync();
                    return true;
                }
                else                
                    Interlocked.Exchange(ref SendIng, 0);

            return false;
        }

        private void SendAsync()
        {
            try
            {                

                if (!_sock.SendAsync(_send))
                {                   
                    BeginSend(_send);
                }
            }
            catch (ObjectDisposedException)
            {
                Free();
                _sock = null;
            }
          
        }

    }
}
