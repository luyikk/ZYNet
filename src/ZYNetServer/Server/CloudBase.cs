using System;
using ZYSocket.Server;
using Autofac;
using Microsoft.Extensions.Logging;
using ZYNet.CloudSystem.Loggine;

namespace ZYNet.CloudSystem.Server
{
    public class CloudBase
    {
        protected ILog Log;
        public ILoggerFactory LoggerFactory { get; protected set; }


        public IContainer Container { get; protected set; }


        public ISocketServer Server { get; protected set; }

        public Func<Exception, bool> ExceptionOut { get; set; }

        protected Random Ran = new Random();

        public int MaxPacksize { get; set; }

        public Func<byte[], byte[]> DecodeingHandler { get; set; }

        public Func<byte[], byte[]> EcodeingHandler { get; set; }

    }
}
