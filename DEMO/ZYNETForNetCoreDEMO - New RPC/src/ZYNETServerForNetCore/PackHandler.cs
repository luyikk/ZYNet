using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.Interfaces;
using ZYNet.CloudSystem.Server;
using Autofac;
using Microsoft.Extensions.Logging;

namespace ZYNETServerForNetCore
{
    /// <summary>
    /// 服务器包处理器
    /// </summary>
    public partial class PackHandler : ControllerBase
    {

        public static List<UserInfo> UserList = new List<UserInfo>();

        public PackHandler(IContainer container, ILoggerFactory loggerFactory) : base(container, loggerFactory)
        {
            
        }

        [TAG(1000)]
        public bool IsLogOn(IASync async,string username, string password)
        {
            UserInfo tmp = new UserInfo()
            {
                UserName = username,
                PassWord = password,
                Token = async.GetAsyncToken()
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
        public async Task<Result> StartDown(IASync async, string url)
        {

            var htmldata = (await async.Get<IClientPack>().DownHtmlAsync(url))?[0]?.Value<byte[]>();

            if (htmldata != null)
            {
                string html = Encoding.UTF8.GetString(htmldata);

                return async.Res(html);

            }


            return async.Res();// or async.RET(null);
        }



        [TAG(2002)]
        public Task<DateTime> GetTime()
        {

            return Task.FromResult(DateTime.Now);
        }

        [TAG(20022)]
        public Task<string> SetMessage(string a,string b)
        {
            return Task.FromResult(a + b);
        }


        [TAG(20023)]
        public Task SetMessage()
        {
            return Task.Run(() =>
            {
                Console.WriteLine("HHHHHHHHHHHHHHHHH");
            });
        }

        [TAG(2003)]
        public void SetPassWord(IASync async,string password)
        {
            UserInfo user = async.UserToken as UserInfo;

            if (user != null)
            {
                user.PassWord = password;
                Console.WriteLine(user.UserName + " Set PassWord:" + password);
            }


        }

        [TAG(2500)]
        public async Task<Result> TestRec(IASync async, int count)
        {
            count--;
            if (count > 1)
            {
                var x = (await async.Get<IClientPack>().TestRecAsync(count))?[0]?.Value<int>();

                if (x != null && x.HasValue)
                {
                    count = x.Value;
                }
            }

            return async.Res(count);
        }

        [TAG(3000)]
        public int Add(IASync async,int count)
        {
            return count + 1;

        }


        [TAG(3002)]
        public void Sub(int num)
        {
            m -= num;
        }



        int m = 0;

        [TAG(3001)]
        public void Add(int num)
        {
            m += num;
        }


        [TAG(3003)]
        public int GetIt()
        {            
            var c = m;
            m = 0;
            return c;
        }



        long num = 0;

        [TAG(3004)]
        public long AddInt(long n)
        {
            num += n;
            return num;
        }


        [TAG(3005)]
        public long Getnum()
        {
            var c = num;
            num = 0;
            return c;
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
