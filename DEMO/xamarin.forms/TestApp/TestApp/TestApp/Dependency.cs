using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using ZYNet.CloudSystem.SocketClient;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client;

namespace TestApp
{
    public static class Dependency
    {
        public static IContainer Container { get; private set; }

        public static void Init()
        {
            var container = new ContainerBuilder();

            container.RegisterType<ConnectionManager>().As<IConnectionManager>();
            container.RegisterType<CloudClient>().AsSelf().SingleInstance();
            Container = container.Build();
        }
    }
}
