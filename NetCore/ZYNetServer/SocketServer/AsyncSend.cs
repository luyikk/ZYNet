using System.Collections.Concurrent;
using ZYSocket.Server;
using System.Net.Sockets;
using System;

namespace ZYSocket.AsyncSend
{
    public class AsyncSend : ISend
    {
        private SocketAsyncEventArgs _send { get; set; }

        private bool SendIng { get; set; }

        private ConcurrentQueue<byte[]> BufferQueue { get; set; }

        private Socket _sock { get; set; }

        protected int BufferLenght { get; set; } = -1;


        public AsyncSend(Socket sock)
        {
            this._sock = sock;
            SendIng = false;
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


            int offset = e.Offset + e.BytesTransferred;

            if (offset < e.Buffer.Length)
            {

                if (BufferLenght > 0)
                {
                    int length = BufferLenght;

                    if (offset + length > e.Buffer.Length)
                        length = e.Buffer.Length - offset;

                    e.SetBuffer(offset, length);
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
                else
                {
                    e.SetBuffer(offset, e.Count - e.Offset - e.BytesTransferred);
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
            else
            {
                if (InitData())
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
                else
                {
                    SendIng = false;
                }
            }

        }

        private void Free()
        {
            _send.SetBuffer(null, 0, 0);
                       
            for (int i = 0; i < BufferQueue.Count; i++)
                BufferQueue.TryDequeue(out byte[]  tmp);
        }

        private bool InitData()
        {             
            if (BufferQueue.TryDequeue(out byte[] data))
            {

                if (BufferLenght <= 0)
                {
                    _send.SetBuffer(data, 0, data.Length);

                    return true;
                }
                else
                {
                    int length = BufferLenght;

                    if (length > data.Length)
                        length = data.Length;

                    _send.SetBuffer(data, 0, length);

                    return true;
                }

            }
            else
                return false;
        }


        public bool Send(byte[] data)
        {
            if (_sock == null)
                return false;

            BufferQueue.Enqueue(data);

            if (!SendIng)
            {
                if (InitData())
                {
                    SendIng = true;
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
                    return true;
                }

            }

            return false;
        }



    }
}
