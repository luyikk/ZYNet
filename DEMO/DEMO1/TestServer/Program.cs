using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Loggine;
using ZYNet.CloudSystem.Server;
namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            LogFactory.AddConsole() ;//注册日记
            CloudServer tmp = new CloudServer("any", 2285, 1000, 1024*128, 1024*1024);//新建服务器引擎,没个SocketAsync对象缓冲区128k,最大能接收1M长度的数据包          
            tmp.Install(typeof(PackHandler));//安装数据包处理器
            tmp.Start();//启动服务
            while (true)
            {
                string msg= Console.ReadLine();

                foreach (var item in PackHandler.UserList)
                {
                    // item.token.CV(3001, msg);
                    item.Token.Disconnect(); //断开所用用户

                }
            }
        }

    }
}
