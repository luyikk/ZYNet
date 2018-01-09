using System;
using System.Collections.Generic;
using System.Text;
using ZYNet.CloudSystem.Server;

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
            cloudServer = new CloudServer("any", 9557, 10, 1024 * 1024, 8 * 1024 * 1024);
            cloudServer.Install(typeof(ServerPackHandler));
        }

        protected CloudServer cloudServer;


        

        public void Start()
        {
            cloudServer.Start();
        }

        public void Stop()
        {
            cloudServer.Pause();
        }



    }
}
