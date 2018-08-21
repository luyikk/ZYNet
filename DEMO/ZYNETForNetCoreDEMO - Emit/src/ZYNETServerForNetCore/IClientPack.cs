using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Frame;


namespace ZYNETServerForNetCore
{
    [Build]
    public interface IClientPack
    {
        [TAG(2001)]
        ResultAwatier DownHtmlAsync(string url);

        [TAG(3001)]
        void Message(string msg);

        [TAG(2500)]
        ResultAwatier TestRecAsync(int count);
    }
}
