using Autofac;
using System;
using ZYNet.CloudSystem.Interfaces;
using ZYNet.CloudSystem.Server.Options;
using Microsoft.Extensions.Logging;

namespace ZYNet.CloudSystem.Server.Bulider
{
    public class ServBuilder : IServBuilder
    {
       private ContainerBuilder Container { get; set; }

        public ServBuilder()
        {
            Container = new ContainerBuilder();

            ConfigureDefaults();
            this.InstallCloudServerType();
        }

        public IServBuilder ConfigureDefaults()
        {

            ConfigureServHostAndPort();
            ConfigureMaxConnectNumber();
            ConfigureBufferSize();
            ConfigureTimeOut();
            ConfigureConnectFiter();
            ConfigureDataEncoding();
            ConfigureLogSet();
            return this;
        }

        public IServBuilder ConfigureServHostAndPort(Action<HostOptions> config = null)
        {
            Container.Register<HostOptions>
                (p =>
                    {
                        var c = new HostOptions();
                        config?.Invoke(c);
                        return c;
                    }
                 ).AsSelf().SingleInstance();

            return this;
        }

        public IServBuilder ConfigureMaxConnectNumber(Action<MaxConnectNumberOptions> config = null)
        {
            Container.Register<MaxConnectNumberOptions>
                (p =>
                {
                    var c = new MaxConnectNumberOptions();
                    config?.Invoke(c);
                    return c;
                }
                 ).AsSelf().SingleInstance();

            return this;
        }


        public IServBuilder ConfigureBufferSize(Action<BufferSizeOptions> config = null)
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

        public IServBuilder ConfigureTimeOut(Action<TimeOutOptions> config = null)
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

        public IServBuilder ConfigureDataEncoding(Action<DataEncodeOptions> config = null)
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

        public IServBuilder ConfigureLogSet(Func<ILoggerFactory> config = null, Action<ILoggerFactory> set = null)
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
                        var log= config();
                        if (set is null)
                            log.AddConsole(LogLevel.Trace);
                        else
                            set(log);
                        return log;
                    }
                   
                }).SingleInstance();

            return this;
        }


        public IServBuilder ConfigureConnectFiter(Func<IComponentContext,IConnectfilter> fun = null)
        {
            if (fun != null)
                Container.Register(p => fun(p)).As<IConnectfilter>().SingleInstance();
            return this;
        }

        public IServBuilder ContainerBuilder(Action<ContainerBuilder> action)
        {
            action?.Invoke(Container);
            return this;
        }

        public CloudServer Bulid()
        {
            var build=  Container.Build();

            return build.Resolve<CloudServer>(new NamedParameter("container", build));
        }

    }
}
