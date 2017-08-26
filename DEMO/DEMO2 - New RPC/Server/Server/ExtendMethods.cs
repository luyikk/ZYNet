using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Server;
using ZYNet.CloudSystem.Frame;

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
        [MethodCmdTag((int)Cmdtype.GetNick)] 
        FiberThreadAwaiter<ReturnResult> GetNick();

        [MethodCmdTag((int)Cmdtype.SetAllUser)]
        void SetUserList(List<UserInfo> userlist);

        [MethodCmdTag((int)Cmdtype.AddUser)]
        void AddUser(UserInfo user);

        [MethodCmdTag((int)Cmdtype.RemoveUser)]
        void RemoveUser(UserInfo user);

        [MethodCmdTag((int)Cmdtype.MessageTo)]
        void MessageTo(string username, string msg);

        [MethodCmdTag((int)Cmdtype.MsgToUser)]
        FiberThreadAwaiter<ReturnResult> MsgToUser(string username, string msg);
    }
}
