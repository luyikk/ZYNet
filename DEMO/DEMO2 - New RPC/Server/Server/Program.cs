using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Loggine;
using ZYNet.CloudSystem.Server;
using ZYNet.CloudSystem.Server.Bulider;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {

            CloudServer tmp = new ServBuilder().ConfigureDefaults().ConfigureServHostAndPort(p=>p.Port= 3775).Bulid();
            tmp.Install(typeof(PackHandler));
            tmp.Start();
            while (true)
            {
                 Console.ReadLine();              
            }
        }

    
    }
}
