using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client;
namespace FileServ.Client
{
    public class ClientPackHander
    {


        [TAG(20000)]
        public async Task Wrile(AsyncCalls client, string msg)
        {          
            await Task.Run(()=> Console.WriteLine(msg));
        }
    }
}
