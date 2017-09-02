using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.Server;

namespace TestServer
{
  
    /// <summary>
    /// 服务器包处理器
    /// </summary>
    public static partial class PackHandler
    {

        /// <summary>
        /// 用户列表
        /// </summary>
        public static List<UserInfo> UserList = new List<UserInfo>();

        /// <summary>
        /// 登入接口
        /// </summary>
        /// <param name="token"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [TAG(1000)]
        public static bool IsLogOn(ASyncToken token, string username, string password)
        {
            UserInfo tmp = new UserInfo() //创建一个用户对象用来保存用户信息
            {
                UserName=username,
                PassWord=password,
                Token=token
            };

            token.UserToken = tmp;
            token.UserDisconnect += Token_UserDisconnect; //注册断开处理
            lock (UserList)
            {
                UserList.Add(tmp); //添加到用户列表
            }

            

            return true;

        }

        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="async"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        [TAG(2001)]
        public static async Task<ReturnResult> StartDown(AsyncCalls async, string url)
        {

            var callback = await async.CR(2001, url); //调用客户端的2001函数 让客户端去下载,PS 老子才不下

            if (callback != null && !callback.IsError) //如果下载成功
            {
                var htmldata = callback?[0]?.Value<byte[]>();

                if (htmldata != null)
                {

                    string html = Encoding.UTF8.GetString(htmldata);

                    return async.RET(html); //返回HTML

                }
            }
            else
            {
                Console.WriteLine(callback.ErrorMsg); //打印错误
            }

            return async.RET();// or async.RET(null); 返回NULL
            
        }


        /// <summary>
        /// 获取服务器时间
        /// </summary>
        /// <param name="async"></param>
        /// <returns></returns>
        [TAG(2002)]
        public static Task<ReturnResult> GetTime(AsyncCalls async)
        {           
            return Task.FromResult<ReturnResult>(async.RET(DateTime.Now)); //返回当前服务器时间
        }

        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="token"></param>
        /// <param name="password"></param>
        [TAG(2003)]
        public static void SetPassWord(ASyncToken token,string password)
        {

            if (token.UserToken is UserInfo user)
            {
                user.PassWord = password;
                Console.WriteLine(user.UserName + " Set PassWord:" + password);

            }


        }

        /// <summary>
        /// 测试递归函数
        /// </summary>
        /// <param name="async"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [TAG(2500)]
        public static async Task<ReturnResult> TestRec(AsyncCalls async,int count)
        {
            count--;
            if (count > 1) //如果大于1 那么调用客户端的 2005 获取结果 返回
            {
                var x = (await async.CR(2500, count))?[0]?.Value<int>();

                if(x!=null&&x.HasValue)
                {
                    count = x.Value;
                }
            }
            
            return async.RET(count);
        }

        /// <summary>
        /// 测试异常
        /// </summary>
        /// <param name="async"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        [TAG(2600)]
        public static  Task<ReturnResult> TestError(AsyncCalls async, int c)
        {
            throw new Exception("EEEE");
        }



        /// <summary>
        /// USER DISCONNECT
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private static void Token_UserDisconnect(ASyncToken arg1, string arg2)
        {
           if(arg1.UserToken!=null)
           {

                if (arg1.UserToken is UserInfo user)
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
