using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Frame;


namespace ZYNETServerForNetCore
{
    public interface IClientPack
    {
        [MethodRun(2001)]
        ResultAwatier DownHtmlAsync(string url);

        [MethodRun(3001)]
        void Message(string msg);

        [MethodRun(2500)]
        ResultAwatier TestRecAsync(int count);
    }
}
