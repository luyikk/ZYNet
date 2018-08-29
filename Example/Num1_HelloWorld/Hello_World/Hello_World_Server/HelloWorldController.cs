using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using ZYNet.CloudSystem.Server;
using ZYNet.CloudSystem;

namespace Hello_World_Server
{
    /// <summary>
    /// 9创建一个HelloWorldController用于客户端调用
    /// 记住此类必须继承ControllerBase
    /// 每个client连接 都会生成一个 此类对象,用于1对1服务
    /// client连接对这个对象操作都是线程安全的
    /// 可以通过this.Container来调用Autofac 对象容器,你也可以在build的时候自定义一些自己的实现
    /// 客户端断线后,重连会保证还是当前对象,当然需要服务器设置多久没重连后清空这些对象,默认为1分钟,可以在build的ConfigureTimeOut设置时间
    /// 客户端是通过sessionkey来保证对象同步的,你可以在客户端设置ISessionRW接口对象来设置sessionkey的保存方式
    /// </summary>
    public class HelloWorldController : ControllerBase
    {
        public HelloWorldController(IContainer container) : base(container)
        {

        }

        [TAG(1)]
        public void MessageShow(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
