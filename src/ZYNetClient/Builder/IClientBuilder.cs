using System;
using Autofac;
using Microsoft.Extensions.Logging;
using ZYNet.CloudSystem.Client.Options;

namespace ZYNet.CloudSystem.Client.Bulider
{
    public interface IClientBuilder
    {
        IContainer Containerbulid { get; }

        CloudClient Bulid();
        IClientBuilder ConfigureBufferSize(Action<BufferSizeOptions> config = null);
        IClientBuilder ConfigureConnectionManager(Func<IComponentContext, IConnectionManager> config = null, Action<IComponentContext, IConnectionManager> set = null);
        IClientBuilder ConfigureDataEncoding(Action<DataEncodeOptions> config = null);
        IClientBuilder ConfigureDefaults();
        IClientBuilder ConfigureLogSet(Func<ILoggerFactory> config = null, Action<ILoggerFactory> set = null);
        IClientBuilder ConfigureSessionRW(Func<ISessionRW> config = null, Action<ISessionRW> set = null);
        IClientBuilder ConfigureTimeOut(Action<TimeOutOptions> config = null);
        IClientBuilder ContainerBuilder(Action<ContainerBuilder> action);
    }
}