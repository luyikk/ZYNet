    using System;
    using Microsoft.Extensions.Logging;


namespace ZYNet.CloudSystem.Loggine
{

    public static class LogFactory
    {
        static readonly ILoggerFactory DefaultFactory;

        static LogFactory()
        {
            DefaultFactory = new LoggerFactory();
        }


        public static void AddConsole() => AddConsoleProvider(LogLevel.Trace);


        public static void AddConsoleProvider(LogLevel mininumLevel = LogLevel.Trace) =>
            DefaultFactory.AddConsole(mininumLevel, false);

        public static void AddProvider(ILoggerProvider provider)
        {
            if (provider == null)
            {
                return;
            }

            DefaultFactory.AddProvider(provider);
        }

        public static ILog ForContext<T>() => ForContext(typeof(T).Name);

        public static ILog ForContext(Type type) => ForContext(type?.Name);

        public static ILog ForContext(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = $"Unkonwn {Guid.NewGuid()}";
            }

            ILogger logger = DefaultFactory.CreateLogger(name);
            return new DefaultLog(logger);
        }
    }

}
