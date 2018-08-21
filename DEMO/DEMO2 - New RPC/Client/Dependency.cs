using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.SocketClient;

namespace Client
{
    public static class Dependency
    {
        public static IContainer Container { get; private set; }

        public static void Register()
        {
            var bullder = new ContainerBuilder();
            bullder.RegisterType<ConnectionManager>().As<IConnectionManager>();
            bullder.RegisterType<CloudClient>().AsSelf().SingleInstance();
            Container = bullder.Build();
        }
    }
}
