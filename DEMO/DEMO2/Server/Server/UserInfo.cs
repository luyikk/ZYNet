using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Server;

namespace Server
{
    [ProtoContract]
    public class UserInfo
    {
        [ProtoMember(1)]
        public string UserName { get; set; }
        [ProtoMember(2)]
        public string Nick { get; set; }

        public ASyncToken token { get; set; }
    }
}
