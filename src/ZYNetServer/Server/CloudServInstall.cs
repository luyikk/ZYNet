using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using ZYNet.CloudSystem.Server.Bulider;
using ZYNet.CloudSystem.Server.Options;
using ZYSocket.Server;

namespace ZYNet.CloudSystem.Server
{
    public static class InstallCloudServ
    {
        public static IServBuilder InstallCloudServerType(this IServBuilder servBuilder)
        {
            servBuilder.ContainerBuilder(p =>
            {
                p.RegisterType<AsyncSend>().As<ISend>();
                p.RegisterType<CloudServer>().SingleInstance();
                p.Register<ZYSocketSuper>(x=>new ZYSocketSuper(x.Resolve<HostOptions>().Host,
                    x.Resolve<HostOptions>().Port,
                    x.Resolve<MaxConnectNumberOptions>().MaxConnNumber,
                    x.Resolve<BufferSizeOptions>().MaxBufferSize)).As<ISocketServer>().SingleInstance();

                p.RegisterType<AsyncMethodDef>().As<IAsyncMethodDef>();
            });

            return servBuilder;
        }
    }
}
