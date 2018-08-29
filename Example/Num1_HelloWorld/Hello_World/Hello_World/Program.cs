using System;
using ZYNet.CloudSystem.Client.Bulider;

namespace Hello_World
{
    class Program
    {
        static void Main(string[] args)
        {
            //1从NUGET 添加ZYNET 最新版本
            //2项目中添加FodyWeavers.xml
            //3修改FodyWeavers.xml文件内容
            //<Weavers>
            //  <InterfaceFodyAddin/>
            //</Weavers>
            //4 using ZYNet.CloudSystem.Client.Bulider


            var client = new ClientBuilder().Bulid(); //5 创建一个client对象

            if (client.Init("127.0.0.1", 7777)) //6连接服务器
            {
                client.Get<IService>().ServerShowMsg("hello word"); //8在服务器上显示hello word

            }
            else
                Console.WriteLine("not connect server");

            Console.ReadKey();

        }

    }

}
