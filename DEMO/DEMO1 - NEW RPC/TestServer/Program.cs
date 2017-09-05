using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Server;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //其他不懂看DEMO1
            LogAction.LogOut += LogAction_LogOut;
            CloudServer tmp = new CloudServer("any", 2285, 1000, 1024*128, 1024*1024);//没个SocketAsync对象缓冲区128k,最大能接收1M长度的数据包
            tmp.Install(typeof(PackHandler));
            tmp.Start();
            while (true)
            {
                string msg= Console.ReadLine();

                foreach (var item in PackHandler.UserList)
                {
                    item.token.Get<IClientPacker>().Message(msg);//返回一个 IClientPacker 同步调用 Message 函数

                }
            }
        }

        private static void LogAction_LogOut(object sender,string msg, LogType type)
        {
            Console.WriteLine(msg);
        }
    }
}
