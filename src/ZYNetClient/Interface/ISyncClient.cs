using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZYNet.CloudSystem.Client
{
    public interface ISyncClient
    {
        bool IsConnect { get; }

        event Action<byte[],int,int> BinaryInput;

        event Action<string> Disconnect;

        bool Connect(string host, int port);
        void Send(byte[] data);
        void Close();
    }
}
