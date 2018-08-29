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

            server.Install()
            
        }
    }
}
