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


            //其他不懂看DEMO1
            LogFactory.AddConsole();
            CloudServer tmp = new CloudServer("any", 2285, 1000, 1024*128, 1024*1024);//没个SocketAsync对象缓冲区128k,最大能接收1M长度的数据包
            tmp.Install(typeof(PackHandler));
            tmp.Start();
            tmp.ReadOutTime = 20000;
            tmp.CheckTimeOut = true;
            while (true)
            {
                string msg= Console.ReadLine();

                foreach (var item in PackHandler.UserList)
                {
                    item.token.Get<IClientPacker>().Message(msg);//返回一个 IClientPacker 同步调用 Message 函数

                }
            }
        }

    }
}
