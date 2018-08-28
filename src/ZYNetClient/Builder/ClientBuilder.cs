using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Microsoft.Extensions.Logging;
using ZYNet.CloudSystem.Client.Options;
using ZYNet.CloudSystem.SocketClient;

namespace ZYNet.CloudSystem.Client.Bulider
{
    public class ClientBuilder : IClientBuilder
    {
        private ContainerBuilder Container { get; set; }

        public IContainer Containerbulid { get; private set; }

        public ClientBuilder()
        {
            Container = new ContainerBuilder();
            this.InstallCloudClientType();
            ConfigureDefaults();
        }

        public IClientBuilder ConfigureDefaults()
        {
            ConfigureBufferSize();
            ConfigureTimeOut();
            ConfigureDataEncoding();
            ConfigureLogSet();
            ConfigureSessionRW();
            ConfigureConnectionManager();
            return this;
        }


        public IClientBuilder ConfigureBufferSize(Action<BufferSizeOptions> config = null)
        {
            Container.Register<BufferSizeOptions>
                (p =>
                {
                    var c = new BufferSizeOptions();
                    config?.Invoke(c);
                    return c;
                }
                 ).AsSelf().SingleInstance();

            return this;
        }

        public IClientBuilder ConfigureTimeOut(Action<TimeOutOptions> config = null)
        {
            Container.Register<TimeOutOptions>
                (p =>
                {
                    var c = new TimeOutOptions();
                    config?.Invoke(c);
                    return c;
                }
                 ).AsSelf().SingleInstance();

            return this;
        }

        public IClientBuilder ConfigureDataEncoding(Action<DataEncodeOptions> config = null)
        {
            Container.Register<DataEncodeOptions>
                (p =>
                {
                    var c = new DataEncodeOptions();
                    config?.Invoke(c);
                    return c;
                }
                 ).AsSelf().SingleInstance();

            return this;
        }

        public IClientBuilder ConfigureLogSet(Func<ILoggerFactory> config = null, Action<ILoggerFactory> set = null)
        {
            Container.Register<ILoggerFactory>
                (p =>
                {
                    if (config is null)
                    {
                        var log = new LoggerFactory();

                        if (set is null)
                            log.AddConsole(LogLevel.Trace);
                        else
                            set(log);

                        return log;
                    }
                    else
                    {
                        var log = config();
                        if (set is null)
                            log.AddConsole(LogLevel.Trace);
                        else
                            set(log);
                        return log;
                    }

                }).SingleInstance();

            return this;
        }


        public IClientBuilder ConfigureSessionRW(Func<ISessionRW> config = null, Action<ISessionRW> set = null)
        {
            Container.Register<ISessionRW>
                (p =>
                {
                    if (config is null)
                    {
                        var log = new SessionRWMemory();
                        set?.Invoke(log);
                        return log;
                    }
                    else
                    {
                        var log = config();
                        set?.Invoke(log);                        
                        return log;
                    }

                }).SingleInstance();

            return this;
        }

        public IClientBuilder ConfigureConnectionManager(Func<IComponentContext,IConnectionManager> config = null, Action<IComponentContext,IConnectionManager> set = null)
        {
            Container.Register<IConnectionManager>
                (p =>
                {
                    if (config is null)
                    {
                        var log = new ConnectionManager(p.Resolve<BufferSizeOptions>().MaxBufferSize, p.Resolve<BufferSizeOptions>().SendBufferSize, p.Resolve<ISessionRW>());
                        set?.Invoke(p, log);
                        return log;
                    }
                    else
                    {
                        var log = config(p);
                        set?.Invoke(p, log);
                        return log;
                    }

                }).SingleInstance();

            return this;
        }




        public IClientBuilder ContainerBuilder(Action<ContainerBuilder> action)
        {
            action?.Invoke(Container);
            return this;
        }

        public CloudClient Bulid()
        {
            Containerbulid = Container.Build();


            return Containerbulid.Resolve<CloudClient>(new NamedParameter("container", Containerbulid));
        }



    }
}
