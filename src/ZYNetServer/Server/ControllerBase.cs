using System;
using Microsoft.Extensions.Logging;
using Autofac;

namespace ZYNet.CloudSystem.Server
{
    public abstract class ControllerBase:IDisposable
    {
        public IContainer Container { get; private set; }
        public ILoggerFactory LoggerFactory { get; private set; }

        public ControllerBase(IContainer container, ILoggerFactory loggerFactory)
        {
            Container = container;
            LoggerFactory = loggerFactory;
        }

        public virtual void Dispose()
        {
            
        }
    }
}
