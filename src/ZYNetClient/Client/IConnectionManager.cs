using System;
using System.Threading.Tasks;
using ZYSocket.share;

namespace ZYNet.CloudSystem.Client
{
    public interface IConnectionManager
    {
        string Host { get; }
        bool IsConnect { get; }
        int MaxBufferLength { get; }
        int Port { get; }
        ZYNetRingBufferPool RingBuffer { get; }
        ISyncClient Sock { get; }

        ISessionRW SessionRW { get;  }

        event Action<byte[]> BinaryInput;
        event Action<string> Disconnect;

        void Close();
        bool Install(string host, int port, int maxBufferLength);
        Task<bool> InstallAsync(string host, int port, int maxBufferLength);
        void SendData(byte[] data);
    }
}