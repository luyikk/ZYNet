using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.Server;


namespace ZYNETServerForNetCore
{
    /// <summary>
    /// 服务器包处理器
    /// </summary>
    public  partial class PackHandler1:ControllerBase
    {

        public  static List<UserInfo> UserList = new List<UserInfo>();

        public string Name { get; set; }

        public PackHandler1(ASyncToken token):base(token)
        {

        }


        [TAG(1000)]
        public  bool IsLogOn(string username, string password)
        {
            UserInfo tmp = new UserInfo()
            {
                UserName = username,
                PassWord = password,
                token = Async.GetAsyncToken()
            };

            Name = username;

            Token.UserToken = tmp;
            Token.UserDisconnect += Token_UserDisconnect;         
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
        public  async Task<string> StartDown(string url)
        {
            
            var htmldata = (await GetForEmit<IClientPack>().DownHtmlAsync(url))?[0]?.Value<byte[]>();

            if (htmldata != null)
                return Encoding.UTF8.GetString(htmldata);

            return null;// or async.RET(null);
        }



        [TAG(2002)]
        public  Task<Result> GetTime()
        {
            
            return Async.ResTask(DateTime.Now);
        }


        [TAG(2003)]
        public  void SetPassWord(string password)
        {
            UserInfo user = Async.UserToken as UserInfo;

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
