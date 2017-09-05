using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Server;
using ZYNet.CloudSystem.Frame;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            LogAction.LogOut += LogAction_LogOut;

            CloudServer tmp = new CloudServer(1024 * 1024);//没个SocketAsync对象缓冲区128k,最大能接收1M长度的数据包
            tmp.Install(typeof(PackHandler));
            tmp.Start();
            while (true)
            {
                 Console.ReadLine();              
            }
        }

        private static void LogAction_LogOut(object sender,string msg, LogType type)
        {
            Console.WriteLine(msg);
        }
    }
}
