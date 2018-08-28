using System;
using System.Collections.Generic;
using System.Text;
using ZYNet.CloudSystem.Server;
using ZYNet.CloudSystem.Server.Bulider;
using Microsoft.Extensions.Logging;
namespace FileServ.Server
{
    public class FileServer
    {
        protected static readonly FileServer server;

        static FileServer()
        {
            server = new FileServer();
        }

        public static FileServer Server => server;

        private FileServer()
        {
           
        }

        protected CloudServer cloudServer;


        

        public void Start(bool isLog)
        {
            void SetLog(ILoggerFactory log)
            {
                if (isLog)
                    log.AddConsole(LogLevel.Trace);
               
            };

            cloudServer = new ServBuilder().ConfigureLogSet(null, SetLog).ConfigureServHostAndPort(p => p.Port = 9557).ConfigureBufferSize(p => p.MaxPackSize = 8 * 1024 * 1024).Bulid();
            cloudServer.Install(typeof(ServerPackHandler));
            cloudServer.Start();
        }

        public void Stop()
        {
            cloudServer.Pause();
        }



    }
}
