using System;
using System.Collections.Generic;
using System.Text;
using ZYNet.CloudSystem.Client.Bulider;
using Autofac;
using ZYNet.CloudSystem.SocketClient;

namespace ZYNet.CloudSystem.Client
{
    public static class CloudClientInstall
    {
        public static IClientBuilder InstallCloudClientType(this IClientBuilder servBuilder)
        {
            servBuilder.ContainerBuilder(p =>
            {
                p.RegisterType<AsyncSend>().As<ISend>();
                p.RegisterType<ModuleDictionary>().As<IModuleDictionary>().SingleInstance();
                p.RegisterType<CloudClient>().SingleInstance();               

            });

            return servBuilder;
        }
    }
}
