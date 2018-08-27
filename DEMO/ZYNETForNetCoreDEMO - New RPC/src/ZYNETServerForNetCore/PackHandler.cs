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
    public partial class PackHandler : ControllerBase
    {

        public static List<UserInfo> UserList = new List<UserInfo>();

        public PackHandler(ASyncToken token) : base(token)
        {
        }


        [TAG(1000)]
        public bool IsLogOn(string username, string password)
        {
            UserInfo tmp = new UserInfo()
            {
                UserName = username,
                PassWord = password,
                Token = Async.GetAsyncToken()
            };

            tmp.Token.UserToken = tmp;
            tmp.Token.UserDisconnect += Token_UserDisconnect;
            lock (UserList)
            {
                UserList.Add(tmp);
            }



            return true;

        }

        [TAG(2001)]
        public async Task<Result> StartDown(string url)
        {

            var htmldata = (await Async.Get<IClientPack>().DownHtmlAsync(url))?[0]?.Value<byte[]>();

            if (htmldata != null)
            {
                string html = Encoding.UTF8.GetString(htmldata);

                return Async.Res(html);

            }


            return Async.Res();// or async.RET(null);
        }



        [TAG(2002)]
        public Task<Result> GetTime()
        {

            return Task.FromResult<Result>(Async.Res(DateTime.Now));
        }


        [TAG(2003)]
        public void SetPassWord(string password)
        {
            UserInfo user = Async.UserToken as UserInfo;

            if (user != null)
            {
                user.PassWord = password;
                Console.WriteLine(user.UserName + " Set PassWord:" + password);
            }


        }

        [TAG(2500)]
        public async Task<Result> TestRec(int count)
        {
            count--;
            if (count > 1)
            {
                var x = (await Async.Get<IClientPack>().TestRecAsync(count))?[0]?.Value<int>();

                if (x != null && x.HasValue)
                {
                    count = x.Value;
                }
            }

            return Async.Res(count);
        }

        [TAG(3000)]
        public int Add(int count)
        {
            return count + 1;

        }

        /// <summary>
        /// USER DISCONNECT
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void Token_UserDisconnect(ASyncToken arg1, string arg2)
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

        public override void Dispose()
        {
            Console.WriteLine("我被释放了");
        }


    }
}
