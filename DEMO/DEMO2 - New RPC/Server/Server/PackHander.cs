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


        [TAG(1000)]
        public static async Task<Result> IsLogOn(AsyncCalls async,string username)
        {
            if (UserList.Find(p => p.UserName == username) == null)
            {

                UserInfo user = new UserInfo()
                {
                    UserName = username,
                    token = async.AsyncUser
                };

                async.UserToken = user;
                async.IsValidate = true;

                user.Nick = (await async.Get<ClientMethods>().GetNick())?.First?.Value<string>();

                async.Get<ClientMethods>().SetUserList(UserList);

                foreach (var item in UserList)
                {
                    item.token.Get<ClientMethods>().AddUser(user);
                }

                async.AsyncUser.UserDisconnect += AsyncUser_UserDisconnect;

                UserList.Add(user);

                return async.Res(true);
            }
            else
                return async.Res(false,"username not use");
        }


        [TAG(2001)]
        public static void SendMessage(ASyncToken token,string msg)
        {
            var userinfo = token.UserToken as UserInfo;

            if (userinfo != null&&token.IsValidate)
            {
                foreach (var item in UserList)
                {
                    item.token.Get<ClientMethods>().MessageTo(userinfo.UserName, msg);
                }
            }
        }

        [TAG(2002)]
        public static async Task<Result> ToMessage(AsyncCalls async,string account,string msg)
        {
            var userinfo = async.Token<UserInfo>();

            if (userinfo != null && async.IsValidate)
            {
                var touser = UserList.Find(p => p.UserName == account);

                if(touser!=null)
                {

                    var cx = touser.token.MakeAsync(async).Get<ClientMethods>();
                    var ret = (await cx.MsgToUser(userinfo.UserName, msg))?.First?.Value<string>();

                    if(ret!=null)
                    {
                        return async.Res(ret);
                    }
                }
            }

            return async.Res();           
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
                        item.token.Get<ClientMethods>().RemoveUser(userinfo);
                    }
                }
            }
        }
    }
}
