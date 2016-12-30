using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Server;
using ZYNet.CloudSystem.Frame;
using ZYSocket.ZYCoroutinesin;

namespace Server
{
    public enum Cmdtype
    {
        GetNick = 1001,
        SetAllUser = 1003,
        AddUser = 1002,
        RemoveUser= 1004,
        MessageTo = 2001,
        MsgToUser=2002,
    }
    

    public interface ClientMethods
    {
        [MethodRun((int)Cmdtype.GetNick)] 
        FiberThreadAwaiter<ReturnResult> GetNick();

        [MethodRun((int)Cmdtype.SetAllUser)]
        void SetUserList(List<UserInfo> userlist);

        [MethodRun((int)Cmdtype.AddUser)]
        void AddUser(UserInfo user);

        [MethodRun((int)Cmdtype.RemoveUser)]
        void RemoveUser(UserInfo user);

        [MethodRun((int)Cmdtype.MessageTo)]
        void MessageTo(string username, string msg);

        [MethodRun((int)Cmdtype.MsgToUser)]
        FiberThreadAwaiter<ReturnResult> MsgToUser(string username, string msg);
    }
}
