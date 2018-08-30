using System;
using ZYNet.CloudSystem.Client.Bulider;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Client;
using Microsoft.Extensions.Logging;
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

                                                                            //在输出窗口输出日记
            var client = new ClientBuilder().ConfigureLogSet(null,p=>p.AddDebug(LogLevel.Trace)).Bulid(); //5 创建一个client对象

            //创建一个对象共服务器访问
            //为什么要这样的设计?因为这样的设计兼容性更广泛,你可以随时在一个对象当中 install(this),来注册自身提供服务,
            //而不需要写一些特定的访问实现,加大了复杂度,比如我们可以在 winform窗体对象里面注册当前窗体对象.
            //如果我install多个对象,多个对象里面都有一样的TAG 咋办?这里采用替换法,后注册的会覆盖前面的TAG
            client.Install(new ClientConroller(client.LoggerFactory)); 

            if (client.Init("127.0.0.1", 777)) //6连接服务器 这里可以是IP地址或者url
            {

                Run(client);

            }
            else
                Console.WriteLine("not connect server");

            Console.ReadKey();

        }

        static async void Run(CloudClient client)
        {
            client.Get<IService>().ServerShowMsg("hello world"); //8在服务器上显示hello world

            var res = (await client.Get<IService>().ServerShowMsg2("hello world")).As<bool>(); //让服务器显示helloworld 方式2
            Console.WriteLine("服务器调用:{0}", res ? "成功" : "失败");


            var stop = System.Diagnostics.Stopwatch.StartNew(); //我们来记个时
                                                                //好了,我们让1W去递归成0吧
            var c = (await client.Get<IService>().NumberToOne(10000)).As<int>();
            stop.Stop();
            Console.WriteLine($"我成功递归到了:{c},耗时:{stop.ElapsedMilliseconds} 毫秒");



            //我们可以在服务器上写个ADD方法传入一个int,让它加1后返回
            stop.Restart();
            int i = 0;
            while (i < 10000)
                i = (await client.Get<IService>().AddOne(i)).As<int>();
            stop.Stop();
            Console.WriteLine($"我ADD到了:{i},耗时:{stop.ElapsedMilliseconds} 毫秒");


            //我们可以给服务器定义一个ADDIT 方法,服务器对象中建立一个int 值,每次调用Addit 就给int加上参数;

            stop.Restart();
            for (int v = 0; v < 100000; v++)
                client.Get<IService>().Addit(v);
            var x =( await client.Get<IService>().Getit()).As<long>();
            stop.Stop();

            Console.WriteLine($"结果为:{x},耗时:{stop.ElapsedMilliseconds} 毫秒");

            //我们测算下并行看看服务器会不会出现线程问题

            stop.Restart();
            var serv = client.Get<IService>();
            Parallel.For(0, 100000, v =>
            {
                serv.Addit(v);
            });
            x = (await client.Get<IService>().Getit()).As<long>();
            stop.Stop();

            Console.WriteLine($"结果为:{x},耗时:{stop.ElapsedMilliseconds} 毫秒");

        }

    }

}
