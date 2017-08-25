using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Frame;


namespace TestServer
{
    /// <summary>
    /// 定义一个客户端调用接口 用来调用客户端
    /// </summary>
    interface IClientPacker
    {
        [MethodRun(2001)]
        ResultAwatier DownHtml(string url);

        [MethodRun(3001)]
        void Message(string msg);

        [MethodRun(2500)]
        ResultAwatier TestRec(int count);
    }
}
