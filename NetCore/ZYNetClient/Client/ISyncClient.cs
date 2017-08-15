using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZYNet.CloudSystem.Client
{
    public interface ISyncClient
    {
        event Action<byte[]> BinaryInput;

        event Action<string> Disconnect;

        bool Connect(string host, int port);
        void Send(byte[] data);

        void Close();
    }
}
