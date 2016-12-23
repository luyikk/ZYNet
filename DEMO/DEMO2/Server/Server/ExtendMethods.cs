using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Server;

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


    public static class ExtendMethods
    {
        public static async Task<string> GetNick(this AsyncCalls async)
        {
            return (await async.CR((int)Cmdtype.GetNick))?.First?.Value<string>();
        }

        public static void SetUserList(this AsyncCalls async,List<UserInfo> userlist)
        {
            async.CV((int)Cmdtype.SetAllUser, userlist);
        }

        public static void AddUser(this ASyncToken token, UserInfo user)
        {
            token.CV((int)Cmdtype.AddUser, user);
        }

        public static void RemoveUser(this ASyncToken token, UserInfo user)
        {
            token.CV((int)Cmdtype.RemoveUser, user.UserName);
        }

        public static void MessageTo(this ASyncToken token, string username,string msg)
        {
            token.CV((int)Cmdtype.MessageTo, username, msg);
        }

        public static async Task<string> MsgToUser(this AsyncCalls async,string username,string msg)
        {
            return (await async.CR((int)Cmdtype.MsgToUser, username, msg))?.First?.Value<string>();
        }
    }
}
