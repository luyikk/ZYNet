using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.SocketClient;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            LogAction.LogOut += LogAction_LogOut;          
            CloudClient client = new CloudClient(new SocketClient(), 500000, 1024 * 1024); //最大数据包能够接收 1M
            PackHander tmp = new PackHander();
            client.Install(tmp);
            client.Disconnect += Client_Disconnect;           

            if (client.Connect("127.0.0.1", 2285))
            {
            
                var ServerPacker = client.Sync.Get<IPacker>();
                


                var isSuccess = ServerPacker.IsLogOn("123123", "3212312")?.First?.Value<bool>();

                var html = ServerPacker.StartDown("http://www.baidu.com").First?.Value<string>();
                Console.WriteLine("BaiduHtml:" + html.Length);

                var time = ServerPacker.GetTime();

                Console.WriteLine("ServerTime:" + time);

                ServerPacker.SetPassWord("3123123");

               
                int? v= ServerPacker.TestRec(1000)?.First?.Value<int>();


                System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
                stop.Start();
                int c = ServerPacker.TestRec2(10000);
                stop.Stop();
                Console.WriteLine("Rec:{0} time:{1} MS", c, stop.ElapsedMilliseconds);

                Console.ReadLine();

            }

        }



        private static void LogAction_LogOut(string msg, LogType type)
        {
            Console.WriteLine(msg);
        }

        private static void Client_Disconnect(string obj)
        {
            Console.WriteLine(obj);
        }
    }
}
