using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client;
using Microsoft.Extensions.Logging;
using ZYNet.CloudSystem.Loggine;

namespace Hello_World
{
    /// <summary>
    /// 我们添加一个客户端的控制器类给服务器调用,这个类不需要继承啥啥哦
    /// </summary>
    public class ClientConroller
    {
        public ILog Log { get; set; }

        public ClientConroller(ILoggerFactory loggerFactory)
        {
            Log = new DefaultLog(loggerFactory.CreateLogger(nameof(ClientConroller)));
        }

        /// <summary>
        /// 显示MSG,显示完后告诉服务器我显示OK了
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [TAG(101)]
        public bool ShowMsg(IASync async, string msg)
        {
            if (Log == null)
                Log =new DefaultLog(async.LoggerFactory.CreateLogger(nameof(ClientConroller)));

            Log.Debug(msg);
            Console.WriteLine(msg);
            return true;
        }

        [TAG(102)]
        public void MyGood(string msg) //我们把加个IASync参数放在第一位,完全没有任何问题,问题是我们这里不需要,所以就不加了
        {
            Console.WriteLine($"我自豪:{msg}");
        }

        /// <summary>
        /// 这代码和服务器上那代码逻辑是一样的,不同之处在于服务器是在调用客户端,而这里是在调用服务器
        /// </summary>
        /// <param name="async"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        [TAG(103)]
        public async Task<int> Number(IASync async, int num) //一样传入IASync对象,放在第一位,重要的事情要说3遍
        {
            num--;
            if (num > 1)
                num = (await async.Get<IService>().NumberRec(num)).As<int>();
            return num;
        }
    }
}
