using System;
using System.Net.Sockets;

namespace ZYSocket.Server
{
    public interface ISocketServer
    {
        BinaryInputHandler BinaryInput { get; set; }
        BinaryInputOffsetHandler BinaryOffsetInput { get; set; }
        ConnectionFilter Connetions { get; set; }
        int GetMaxBufferSize { get; }
        int GetMaxUserConnect { get; }
        bool IsOffsetInput { get; set; }
        MessageInputHandler MessageInput { get; set; }
        bool NoDelay { get; set; }
        int ReceiveTimeout { get; set; }
        int SendTimeout { get; set; }
        Socket Sock { get; }

        event EventHandler<LogOutEventArgs> MessageOut;

        void Disconnect(Socket socks);
        void Dispose();
        void Send(ISend player, byte[] data);
        void Start();
        void Stop();
    }
}