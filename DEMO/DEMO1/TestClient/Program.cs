using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.SockClient;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            LogAction.LogOut += LogAction_LogOut;
            SocketClient socket = new SocketClient();
            CloudClient client = new CloudClient(socket, 500000, 1024 * 1024); //最大数据包能够接收 1M
            PackHander tmp = new PackHander();
            client.Install(tmp);
            client.Disconnect += Client_Disconnect;           

            if (client.Connect("127.0.0.1", 2285))
            {
                ZYSync Sync = client.Sync;

                var res= Sync.CR(1000, "AAA", "BBB")?[0]?.Value<bool>();

                if(res!=null&&res==true)
                {

                    var html= Sync.CR(2001, "http://www.baidu.com")?[0]?.Value<string>();
                    if (html != null)
                    {
                        Console.WriteLine("BaiduHtml:" + html.Length);

                        var time = Sync.CR(2002)?.First?.Value<DateTime>();

                        Console.WriteLine("ServerTime:" + time);

                        Sync.CV(2003, "123123");

                        var x = Sync.CR(2001, "http://www.qq.com");

                        Console.WriteLine("QQHtml:" + x.First.Value<string>().Length);
                    }
                }

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
