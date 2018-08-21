using System;
using System.Collections.Generic;
using System.Text;
using ZYNet.CloudSystem;

namespace FileServ.Server
{
    [Build]
    public interface IClient
    {
        [TAG(20000)]
        void Wrile(string msg);
    }
}
