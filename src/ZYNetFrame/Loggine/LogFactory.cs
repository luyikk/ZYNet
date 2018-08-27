using System;
using Microsoft.Extensions.Logging;


namespace ZYNet.CloudSystem.Loggine
{

    public static class LogFactory
    {
        private static ILoggerFactory DefaultFactory = new LoggerFactory();

        public static void SetLogFactory(ILoggerFactory loggerFactory)
        {
            DefaultFactory = loggerFactory;
        }

        public static ILoggerFactory AddConsole() => AddConsoleProvider(LogLevel.Trace);
        public static ILoggerFactory AddDebug() => AddDebug(LogLevel.Trace);

        public static ILoggerFactory AddConsoleProvider(LogLevel mininumLevel = LogLevel.Trace) =>
            DefaultFactory.AddConsole(mininumLevel, false);


        public static ILoggerFactory AddDebug(LogLevel mininumLevel = LogLevel.Trace) =>
            DefaultFactory.AddDebug(mininumLevel);

        public static ILoggerFactory AddProvider(ILoggerProvider provider)
        {

            if (provider != null)
                DefaultFactory.AddProvider(provider);
            return DefaultFactory;
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
