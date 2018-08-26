using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Loggine;
using ZYNet.CloudSystem.Server;

namespace ZYNETServerForNetCore
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            LogFactory.AddConsole();
            CloudServer tmp = new CloudServer("any", 2285, 1000, 1024 * 128, 1024 * 1024);//没个SocketAsync对象缓冲区128k,最大能接收1M长度的数据包
            tmp.Install(typeof(PackHandler1));
            tmp.Install(typeof(PackHandler2));
            tmp.Start();
            tmp.CheckTimeOut = false;
            


            while (true)
            {
                string msg = Console.ReadLine();

                foreach (var item in PackHandler1.UserList)
                {
                    item.token.Action(3001, msg);

                }
            }
        }

      
    }

    public class UserInfo
    {
        public string UserName { get; set; }

        public string PassWord { get; set; }

        public ASyncToken token { get; set; }
    }
}
