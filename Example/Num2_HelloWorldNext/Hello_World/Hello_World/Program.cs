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

            //创建一个对象共服务器访问
            //为什么要这样的设计?因为这样的设计兼容性更广泛,你可以随时在一个对象当中 install(this),来注册自身提供服务,
            //而不需要写一些特定的访问实现,加大了复杂度,比如我们可以在 winform窗体对象里面注册当前窗体对象.
            //如果我install多个对象,多个对象里面都有一样的TAG 咋办?这里采用替换法,后注册的会覆盖前面的TAG
            client.Install(new ClientConroller()); 

            if (client.Init("127.0.0.1", 777)) //6连接服务器 这里可以是IP地址或者url
            {
                client.Get<IService>().ServerShowMsg("hello world"); //8在服务器上显示hello world

                var res= client.Get<IService>().ServerShowMsg2("hello world"); //让服务器显示helloworld 方式2
                Console.WriteLine("服务器调用:{0}", res ? "成功" : "失败");

            }
            else
                Console.WriteLine("not connect server");

            Console.ReadKey();

        }

    }

}
