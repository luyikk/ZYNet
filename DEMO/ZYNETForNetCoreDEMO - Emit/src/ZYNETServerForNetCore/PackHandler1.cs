using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.Interfaces;
using ZYNet.CloudSystem.Server;
using Autofac;

namespace ZYNETServerForNetCore
{
    /// <summary>
    /// 服务器包处理器
    /// </summary>
    public  partial class PackHandler1:ControllerBase
    {

        public  static List<UserInfo> UserList = new List<UserInfo>();

        public string Name { get; set; }

        public PackHandler1(IContainer container ):base(container)
        {

        }


        [TAG(1000)]
        public  bool IsLogOn(IASync async, string username, string password)
        {
            UserInfo tmp = new UserInfo()
            {
                UserName = username,
                PassWord = password,
                Token = async.GetAsyncToken()
            };

            Name = username;

            async.UserToken = tmp;
            async.UserDisconnect += Token_UserDisconnect;         
            lock (UserList)
            {
                UserList.Add(tmp);
            }

            return true;

        }

        [TAG(1005)]
        public  async Task ShowMessage(string msg)
        {
            await Task.Run(() =>
            {
                Console.WriteLine(msg);
            });
        }

        [TAG(2001)]
        public  async Task<string> StartDown(IASync async,string url)
        {
            
            var htmldata = (await async.GetForEmit<IClientPack>().DownHtmlAsync(url))?[0]?.Value<byte[]>();

            if (htmldata != null)
                return Encoding.UTF8.GetString(htmldata);

            return null;// or async.RET(null);
        }



        [TAG(2002)]
        public  Task<DateTime> GetTime()
        {
            
            return Task.FromResult(DateTime.Now);
        }


        [TAG(2003)]
        public  void SetPassWord(IASync async, string password)
        {
            UserInfo user = async.UserToken as UserInfo;

            if (user != null)
            {
                user.PassWord = password;
                Console.WriteLine(user.UserName + " Set PassWord:" + password);
            }


        }

        [TAG(4001)]
        public Task<string> GetName()
        {
            return Task.FromResult(Name);
        }


        /// <summary>
        /// USER DISCONNECT
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private  void Token_UserDisconnect(IASync arg1, string arg2)
        {
            if (arg1.UserToken != null)
            {                

                UserInfo user = arg1.UserToken as UserInfo;

                if (user != null)
                {
                    lock (UserList)
                    {
                        UserList.Remove(user);
                    }
                }
            }
        }


    }
}
