using System;
using Autofac;
using Microsoft.Extensions.Logging;
using ZYNet.CloudSystem.Interfaces;
using ZYNet.CloudSystem.Server.Options;

namespace ZYNet.CloudSystem.Server.Bulider
{
    public interface IServBuilder
    {
        CloudServer Bulid();
        IServBuilder ConfigureBufferSize(Action<BufferSizeOptions> config = null);
        IServBuilder ConfigureConnectFiter(Func<IComponentContext, IConnectfilter> fun = null);
        IServBuilder ConfigureDataEncoding(Action<DataEncodeOptions> config = null);
        IServBuilder ConfigureDefaults();
        IServBuilder ConfigureLogSet(Func<ILoggerFactory> config = null, Action<ILoggerFactory> set = null);
        IServBuilder ConfigureMaxConnectNumber(Action<MaxConnectNumberOptions> config = null);
        IServBuilder ConfigureServHostAndPort(Action<HostOptions> config = null);
        IServBuilder ConfigureTimeOut(Action<TimeOutOptions> config = null);
        IServBuilder ContainerBuilder(Action<ContainerBuilder> action);
    }
}