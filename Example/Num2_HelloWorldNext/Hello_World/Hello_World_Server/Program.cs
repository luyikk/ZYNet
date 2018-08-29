using System;
using ZYNet.CloudSystem.Server.Bulider;

namespace Hello_World_Server
{
    class Program
    {
        
        static void Main(string[] args)
        {
            //8 添加一个 Server 基本和 client类似

            var server = new ServBuilder().ConfigureServHostAndPort(p => p.Port = 777).Bulid();//我们绑定本地IP地址和端口777

            server.Install(typeof(HelloWorldController));// 安装HelloWorldController以供服务client

            //你也可以这样写
            //server.Install(typeof(HelloWorldController).Assembly);
            //这样的好处在于你有多个Controller你不需要一一注册了,只要它们处于同一个程序集

            server.Start(); //最后调用start()让服务器启动

            Console.ReadKey();

        }
    }
}
