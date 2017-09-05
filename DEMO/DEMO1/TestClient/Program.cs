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
            LogAction.LogOut += LogAction_LogOut; //注册日记输出事件 用于打印日记          
            CloudClient client = new CloudClient(new SocketClient(), 500000, 1024 * 1024); //创建一个客户端实例，最大数据包能够接收 1M
            PackHander tmp = new PackHander(); //新建一个 客户端逻辑处理器
            client.Install(tmp); //安装
            client.Disconnect += Client_Disconnect;     //注册断开事件      

            if (client.Connect("127.0.0.1", 2285)) //连接到服务器
            {
                ZYSync sync = client.Sync; //返回同步回调

                var res= sync.CR(1000, "AAA", "BBB")?[0]?.Value<bool>(); //调用服务器函数1000，穿入AAA BBB 返回一个布尔值

                if(res!=null&&res==true) //如果结果不等于NULL 并且登入成功
                {
                    

                    var html= sync.CR(2001, "http://www.baidu.com")?[0]?.Value<string>();//调用服务器2001函数 输入一个URL 返回HTML 内容
                    if (html != null) //如果HTML 不等于NULL
                    {
                        Console.WriteLine("BaiduHtml:" + html.Length); //输出HTML长度

                        var time = sync.CR(2002)?.First?.Value<DateTime>(); //调用2002函数 获取当前时间

                        Console.WriteLine("ServerTime:" + time); //打印时间

                        sync.CV(2003, "123123"); //调用2003函数 穿入123123

                        var x = sync.CR(2001, "http://www.qq.com");  //调用服务器2001函数 输入一个URL 返回HTML 内容

                        Console.WriteLine("QQHtml:" + x.First.Value<string>().Length); //输出HTML长度

                        var recx = sync.CR(2500, 100)?.First?.Value<int>(); //调用2500递归函数 输入100 返回1

                        System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch(); //测试调用时间
                        stop.Start();
                        var rec = sync.CR(2500, 10000)?.First?.Value<int>();  // 调用2500递归函数 输入10000 返回1
                        stop.Stop();
                        if (rec != null)
                        {
                            Console.WriteLine("Rec:{0} time:{1} MS",rec.Value,stop.ElapsedMilliseconds); //打印需要的时间
                        }

                        RunTest(client); //测试异步调用

                        //RunTestError(client); //测试异常处理
                    }
                }

                Console.WriteLine("Close");

                Console.ReadLine();
            }

        }

        public static async void RunTest(CloudClient client)
        {

            System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
            stop.Start();
            var rec = (await client.NewAsync().CR(2500, 10000))?.First?.Value<int>(); //异步调用2500  输入10000 返回1
            stop.Stop();
            if (rec != null)
            {
                Console.WriteLine("ASYNC Rec:{0} time:{1} MS", rec.Value, stop.ElapsedMilliseconds); //打印需要的时间
            }
        }


        public static async void RunTestError(CloudClient client)
        {
            var rec = (await client.NewAsync().CR(2600, 10000)); //测试异常处理函数

            if (rec.ErrorId!=0)
            {
                try
                {
                   
                    rec?.As<int>(0);
                   
                }
                catch (ZYNETException cc)
                {
                    Console.WriteLine(cc.ErrorMsg);
                }
                Console.WriteLine(rec.ErrorMsg);
            }
        }

        private static void LogAction_LogOut(object sender, string msg, LogType type)
        {
            Console.WriteLine(msg);
        }

        private static void Client_Disconnect(string obj)
        {
            Console.WriteLine(obj);
        }
    }
}
