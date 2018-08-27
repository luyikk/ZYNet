using System;
using System.Net;

namespace ZYNet.CloudSystem.Interfaces
{
    public interface IConnectfilter
    {
        bool Filter(IPEndPoint point);
    }
}
