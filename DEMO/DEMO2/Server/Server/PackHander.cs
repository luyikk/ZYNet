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
    public static class PackHandler
    {
        public static List<UserInfo> UserList { get; set; } = new List<UserInfo>();


        [MethodRun(1000)]
        public static async Task<ReturnResult> IsLogOn(AsyncCalls async,string username)
        {
            if (UserList.Find(p => p.UserName == username) == null)
            {

                UserInfo user = new UserInfo()
                {
                    UserName = username,
                    token = async.AsyncUser
                };

                async.Token = user;

                user.Nick = (await async.CR(1001))?.First?.Value<string>();

                async.CV(1003, UserList);

                foreach (var item in UserList)
                {
                    item.token.CV(1002, user);
                }

                async.AsyncUser.UserDisconnect += AsyncUser_UserDisconnect;

                UserList.Add(user);

                return async.RET(true);
            }
            else
                return async.RET(false,"username not use");
        }


        [MethodRun(2001)]
        private static void SendMessage(ASyncToken token,string msg)
        {
            var userinfo = token.UserToken as UserInfo;

            if (userinfo != null)
            {
                foreach (var item in UserList)
                {
                    item.token.CV(2001, userinfo.UserName,msg);
                }
            }
        }

        [MethodRun(2002)]
        private static void ToMessage(ASyncToken token,string account,string msg)
        {
            var userinfo = token.UserToken as UserInfo;

            if (userinfo != null)
            {
                var touser = UserList.Find(p => p.UserName == account);

                if(touser!=null)
                {
                    touser.token.CV(2002, userinfo.UserName, msg);
                }
            }

        }



        private static void AsyncUser_UserDisconnect(ASyncToken arg1, string arg2)
        {
            var userinfo = arg1.UserToken as UserInfo;

            if (userinfo != null)
            {
                lock (UserList)
                {
                    if (UserList.Contains(userinfo))
                        UserList.Remove(userinfo);

                    foreach (var item in UserList)
                    {
                        item.token.CV(1004, userinfo.UserName);
                    }
                }
            }
        }
    }
}
