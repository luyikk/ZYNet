using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using ZYNet.CloudSystem.SocketClient;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client.Bulider;

namespace TestApp
{
    public static class Dependency
    {
        public static IContainer Container { get; private set; }

        public static void Init()
        {
            var container = new ClientBuilder().ConfigureTimeOut(p => p.IsCheckAsyncTimeOut = true);
            container.Bulid();
            Container = container.Containerbulid;
        }
    }
}
