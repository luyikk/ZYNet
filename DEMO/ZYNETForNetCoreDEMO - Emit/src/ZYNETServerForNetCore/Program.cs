using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Loggine;
using ZYNet.CloudSystem.Server;
using ZYNet.CloudSystem.Server.Bulider;
namespace ZYNETServerForNetCore
{
    public class Program
    {
       static  void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;


            var tmp = new ServBuilder()
                .ConfigureDefaults()
                .ConfigureServHostAndPort(p => p.Port = 2285)
                .ConfigureLogSet(() => new LoggerFactory(), log => log.AddConsole(LogLevel.Trace))
                .Bulid();          

            tmp.Install(typeof(PackHandler1).Assembly);        
            tmp.Start();
            tmp.IsCheckReadTimeOut = false;
            


            while (true)
            {
                string msg = Console.ReadLine();

                foreach (var item in PackHandler1.UserList)
                {
                    item.Token.Action(3001, msg);

                }
            }
        }

      
    }

    public class UserInfo
    {
        public string UserName { get; set; }

        public string PassWord { get; set; }

        public ASyncToken Token { get; set; }
    }
}
