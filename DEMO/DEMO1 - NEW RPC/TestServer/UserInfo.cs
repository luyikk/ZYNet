using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Server;

namespace TestServer
{
    public class UserInfo
    {
        public string UserName { get; set; }

        public string PassWord { get; set; }

        public ASyncToken token { get; set; }
    }
}
