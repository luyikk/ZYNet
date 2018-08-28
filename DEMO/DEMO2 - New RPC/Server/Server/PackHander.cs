using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Server;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.Interfaces;
using Autofac;

namespace Server
{
    public  class PackHandler: ControllerBase
    {

        public static List<UserInfo> UserList { get; set; } = new List<UserInfo>();


        public PackHandler(IContainer container):base(container)
        {

        }

        [TAG(1000)]
        public  async Task<bool> IsLogOn(IASync async,string username)
        {
            if (UserList.Find(p => p.UserName == username) == null)
            {

                UserInfo user = new UserInfo()
                {
                    UserName = username,
                    token = async.GetAsyncToken()
                };

                async.UserToken = user;
                async.IsValidate = true;

                user.Nick = (await async.Get<ClientMethods>().GetNick()).As<string>();

                async.Get<ClientMethods>().SetUserList(UserList);

                foreach (var item in UserList)
                {
                    item.token.Get<ClientMethods>().AddUser(user);
                }

                async.UserDisconnect += AsyncUser_UserDisconnect;

                UserList.Add(user);

                return true;
            }
            else
                return false;
        }


        [TAG(2001)]
        public  void SendMessage(IASync async,string msg)
        {
            var userinfo = async.UserToken as UserInfo;

            if (userinfo != null&& async.IsValidate)
            {
                foreach (var item in UserList)
                {
                    item.token.Get<ClientMethods>().MessageTo(userinfo.Nick, msg);
                }
            }
        }

        [TAG(2002)]
        public  async Task<string> ToMessage(IASync async,string account,string msg)
        {
            var userinfo = async.Token<UserInfo>();

            if (userinfo != null && async.IsValidate)
            {
                var touser = UserList.Find(p => p.UserName == account);

                if(touser!=null)
                {

                    var cx = touser.token.MakeAsync(async as AsyncCalls).Get<ClientMethods>();
                    var ret = (await cx.MsgToUser(userinfo.Nick, msg)).As<string>();

                    if(ret!=null)
                    {
                        return ret;
                    }
                }
            }

            return null;  
        }



        private  void AsyncUser_UserDisconnect(ASyncToken arg1, string arg2)
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
