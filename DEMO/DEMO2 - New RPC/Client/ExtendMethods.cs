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


    [Build]
    public interface IServerMethods
    {
        [TAG((int)Cmdtype.LogOn)]
        FiberThreadAwaiter<Result> LogOn(string username);

        [TAG((int)Cmdtype.SendAll)]
        void SendMessageToAllUser(string msg);

        [TAG((int)Cmdtype.SendTo)]
        FiberThreadAwaiter<Result> SendMsgToUser(string account, string msg);

    }
}
