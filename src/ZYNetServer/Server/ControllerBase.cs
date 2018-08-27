using System;
using System.Collections.Generic;
using System.Text;
using ZYNet.CloudSystem.Interfaces;

namespace ZYNet.CloudSystem.Server
{
    public abstract class ControllerBase:IDisposable
    {
        public AsyncCalls CurrentAsync { get; set; }
        public ASyncToken Token { get; private set; }

        public ControllerBase(ASyncToken token)
        {
            Token = token;
        }

        public IASync Async {
            get               
            {
                if (CurrentAsync == null)
                    return Token;
                else
                    return CurrentAsync;
            }
        }

        public T Get<T>()
        {
            return Async.Get<T>();
        }

        public T GetForEmit<T>()
        {
            return Async.GetForEmit<T>();
        }

        public virtual void Dispose()
        {
            
        }
    }
}
