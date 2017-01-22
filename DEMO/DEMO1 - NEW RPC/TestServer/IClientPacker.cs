using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Frame;


namespace TestServer
{
    interface IClientPacker
    {
        [MethodRun(2001)]
        FiberThreadAwaiter<ReturnResult> DownHtml(string url);

        [MethodRun(3001)]
        void Message(string msg);

        [MethodRun(2500)]
        FiberThreadAwaiter<ReturnResult> TestRec(int count);
    }
}
