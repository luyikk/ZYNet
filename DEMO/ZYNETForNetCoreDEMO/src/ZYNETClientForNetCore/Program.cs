using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.Loggine;
using ZYNet.CloudSystem.SocketClient;

namespace ZYNETClientForNetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
           
            LogFactory.AddConsole();
            CloudClient client = new CloudClient(new SocketClient(), 500000, 1024 * 1024); //最大数据包能够接收 1M
            PackHander tmp = new PackHander();
            client.Install(tmp);
            client.Disconnect += Client_Disconnect;
            client.CheckAsyncTimeOut = true;
            if (client.Connect("127.0.0.1", 2285))
            {
                ZYSync sync = client.Sync;

                var res = sync.Func(1000, "AAA", "BBB")?[0]?.Value<bool>();

                if (res != null && res == true)
                {

                    var html = sync.Func(2001, "http://www.baidu.com")?[0]?.Value<string>();
                    if (html != null)
                    {
                        Console.WriteLine("BaiduHtml:" + html.Length);

                        var time = sync.Func(2002)?.First?.Value<DateTime>();

                        Console.WriteLine("ServerTime:" + time);

                        sync.Action(2003, "123123");

                        var x = sync.Func(2001, "http://www.qq.com");

                        Console.WriteLine("QQHtml:" + x.First.Value<string>().Length);

                        System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
                        stop.Start();
                        var rec = sync.Func(2500, 10000)?.First?.Value<int>();
                        stop.Stop();
                        if (rec != null)
                        {
                            Console.WriteLine("Rec:{0} time:{1} MS", rec.Value, stop.ElapsedMilliseconds);
                        }

                        TestRun(client);
                    }
                }

                Console.WriteLine("Close");

                Console.ReadLine();
            }

        }


        public static async void TestRun(CloudClient client)
        {
            

            System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
            stop.Start();
            var rec = (await client.NewAsync().Func(2500, 10000))?.First?.Value<int>();
            stop.Stop();
            if (rec != null)
            {
                Console.WriteLine("Async Rec:{0} time:{1} MS", rec.Value, stop.ElapsedMilliseconds);
            }

        }

    
        private static void Client_Disconnect(string obj)
        {
            Console.WriteLine(obj);
        }
    }
}
