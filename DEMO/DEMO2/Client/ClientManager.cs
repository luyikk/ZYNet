using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.SocketClient;

namespace Client
{
    public static class ClientManager
    {
        public static CloudClient Client { get; private set; }

        public static event Action<string> Dissconnect;
      

        public static string  Host { get; set; }
        public static int Port { get; set; }

        public static ZYSync Sync => Client?.Sync;

        public static bool IsConnect { get; set; } = false;


        public static bool Connect(string host,int port)
        {
            Host = host;
            Port = port;

            Init();

            if(Client.Connect(host,port))
            {
                IsConnect = true;
                return true;
            }

            return false;
        }

        public static void Init()
        {           
            Client = new CloudClient(new SocketClient(), 5000, 1024 * 1024);
            Client.Disconnect += Client_Disconnect;           
        }

       

        public static bool ReConnect()
        {
            Init();

            if (Client.Connect(Host, Port))
            {
                return true;
            }

            return false;
        }


        private static void Client_Disconnect(string obj)
        {
            IsConnect = false;
            if (Dissconnect!=null)
            {
                Dissconnect(obj);
            }
        }
    }
}
