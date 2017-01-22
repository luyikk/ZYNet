using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem;


namespace Client
{
    public enum Cmdtype
    {
        LogOn=1000,
        SendAll=2001,
        SendTo= 2002,
    }

    

    public interface ServerMethods
    {
        [MethodRun((int)Cmdtype.LogOn)]
        FiberThreadAwaiter<ReturnResult> LogOn(string username);

        [MethodRun((int)Cmdtype.SendAll)]
        void SendMessageToAllUser(string msg);

        [MethodRun((int)Cmdtype.SendTo)]
        FiberThreadAwaiter<ReturnResult> SendMsgToUser(string account, string msg);

    }
}
