using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    [ProtoContract]
    public class UserInfo
    {
        [ProtoMember(1)]
        public string UserName { get; set; }

        [ProtoMember(2)]
        public string Nick { get; set; }

        public override string ToString()
        {
            return Nick.ToString();
        }
    }
}
