﻿using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using ZYNet.CloudSystem.Server;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Loggine;
using Microsoft.Extensions.Logging;

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
        private readonly ILog Log;


        /// <summary>
        /// 构造函数里面有2个参数
        /// 这些参数对你来说都是有用的
        /// container 为autofac 对象容器,通过它你可以在build的前配置一些自定义功能,供自己使用
        /// loggerFactory 为日记输出工厂类,通过它你可以自定义一些日记输出便于调试,默认日记工厂类为微软的LoggerFactory,
        /// 你当然也可以在build之前设置成其他的比如log4net
        /// </summary>
        /// <param name="container">autofac 对象容器</param>
        /// <param name="loggerFactory">日记输出系统</param>
        public HelloWorldController(IContainer container, ILoggerFactory loggerFactory) : base(container, loggerFactory)
        {
            Log =new DefaultLog(loggerFactory.CreateLogger(nameof(HelloWorldController))); //我们在这里设置了一个内置日记输出对象
        }

        /// <summary>
        /// 这个方法就是用来给客户端调用显示helloword的方法
        /// msg就是需要我们打印到控制台上面的字符
        /// </summary>
        /// <param name="msg"></param>
        [TAG(1)]
        public void MessageShow(string msg)
        {
            Console.WriteLine(msg);

            //在打印完,我们让log out下
            Log.Debug($"客户端让我打印 {msg}");
        }



    }
}
