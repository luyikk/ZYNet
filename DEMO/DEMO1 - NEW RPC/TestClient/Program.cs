using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.SocketClient;
using ZYNet.CloudSystem.Loggine;
namespace TestClient
{
    class Program
    {
        /// <summary>
        /// 其他不懂看DEMO 1
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

           

            LogFactory.AddConsole();
            CloudClient client = new CloudClient(new SocketClient(), 500000, 1024 * 1024); //最大数据包能够接收 1M
            PackHander tmp = new PackHander();
            client.Install(tmp);
            client.Disconnect += Client_Disconnect;
            client.CheckAsyncTimeOut = true;
            if (client.Connect("127.0.0.1", 2285))
            {

                var serverPacker = client.Sync.Get<IPacker>(); //获取一个 IPACKER 实例 用来调用服务器               


                var isSuccess = serverPacker.IsLogOn("123123", "3212312")?.First?.Value<bool>(); //调用服务器的isLOGON函数

                var html = serverPacker.StartDown("http://www.baidu.com").First?.Value<string>(); //调用服务器的StartDown 函数
                Console.WriteLine("BaiduHtml:" + html.Length);

                var time = serverPacker.GetTime();//调用服务器的GetTime 函数

                Console.WriteLine("ServerTime:" + time);

                serverPacker.SetPassWord("3123123"); //调用服务器的SetPassWord 函数

                System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
                stop.Start();
                int c = serverPacker.TestRec2(10000);
                stop.Stop();
                Console.WriteLine("Rec:{0} time:{1} MS", c, stop.ElapsedMilliseconds);

                RunTest(client);

                Console.ReadLine();

            }

        }


        public static async void RunTest(CloudClient client)
        {
            var server = client.NewAsync().Get<IPacker>();

             int? v = (await server.TestRecAsync(2))?[0]?.Value<int>();

            System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
            stop.Start();
            var res = (await server.TestRecAsync(10000));

            bool? isError = res?.IsError;
            if(isError.HasValue&&isError.Value)
            {
                Console.WriteLine($"ERROR:{res.ErrorMsg}");
                return;
            }
            int? c =res?.First?.Value<int>();
            stop.Stop();

            if (c.HasValue)
                Console.WriteLine("ASync Rec:{0} time:{1} MS", c, stop.ElapsedMilliseconds);


        }



        private static void Client_Disconnect(string obj)
        {
            Console.WriteLine(obj);
        }
    }
}
