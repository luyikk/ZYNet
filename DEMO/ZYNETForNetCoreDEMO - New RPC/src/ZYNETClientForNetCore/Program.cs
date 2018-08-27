using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.SocketClient;
using ZYNet.CloudSystem.Loggine;

namespace ZYNETClientForNetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
           
            CloudClient client = new CloudClient(new ConnectionManager(), 60000, 1024 * 1024); //最大数据包能够接收 1M
            PackHander tmp = new PackHander();
            client.Install(tmp);
            client.Disconnect += Client_Disconnect;
            client.CheckAsyncTimeOut = false;

            if (client.Init("127.0.0.1", 2285))
            {

                IPacker ServerPack = client.Get<IPacker>();


                re:
                try
                {
                    var res = ServerPack.IsLogOn("AAA", "BBB")?[0]?.Value<bool>();

                    if (res != null && res == true)
                    {

                        var html = ServerPack.StartDown("http://www.baidu.com")?[0]?.Value<string>();
                        if (html != null)
                        {
                            Console.WriteLine("BaiduHtml:" + html.Length);

                            var time = ServerPack.GetTime();

                            Console.WriteLine("ServerTime:" + time);

                            ServerPack.SetPassWord("123123");

                            var x = ServerPack.StartDown("http://www.qq.com");

                            Console.WriteLine("QQHtml:" + x.First.Value<string>().Length);

                            System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
                            stop.Start();
                            var rec = ServerPack.TestRec2(10000);
                            stop.Stop();




                            Console.WriteLine("Rec:{0} time:{1} MS", rec, stop.ElapsedMilliseconds);
                            TestRun(client);

                        }
                    }
                }
                catch (TimeoutException er)
                {
                    Console.WriteLine(er.ToString());
                }

                Console.WriteLine("Close");
                Console.ReadLine();


            }

        }


        public static async void TestRun(CloudClient client)
        {
            var Server = client.Get<IPacker>();

            var test = (await Server.TestRecAsync(10))?[0]?.Value<int>();
            Console.WriteLine(test);

            System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
            stop.Start();
            var rec = await Server.TestRecAsync(10000);
            stop.Stop();

            Console.WriteLine("Async Rec:{0} time:{1} MS", rec.First.Value<int>(), stop.ElapsedMilliseconds);

            stop.Restart();

            long i = 0;
            while (i < 100000)
                i = (await Server.Add(i)).As<long>();

            stop.Stop();

            Console.WriteLine("Async Add:{0} time:{1} MS",i, stop.ElapsedMilliseconds);
        }




        private static void Client_Disconnect(string obj)
        {
            Console.WriteLine(obj);
        }
    }
}
