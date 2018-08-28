using System;

using Autofac;

namespace ZYNet.CloudSystem.Server
{
    public abstract class ControllerBase:IDisposable
    {
        public IContainer Container { get; set; }

        public ControllerBase(IContainer container)
        {
            Container = container;
        }

        public virtual void Dispose()
        {
            
        }
    }
}
