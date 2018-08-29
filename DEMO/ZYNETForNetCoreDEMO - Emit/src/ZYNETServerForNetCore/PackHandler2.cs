using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.Server;
using Autofac;
using ZYNet.CloudSystem.Interfaces;
using Microsoft.Extensions.Logging;

namespace ZYNETServerForNetCore
{
    public class PackHandler2:ControllerBase
    {
        public PackHandler2(IContainer container, ILoggerFactory loggerFactory) : base(container, loggerFactory)
        {
        }


        [TAG(2500)]
        public async Task<int> TestRec(IASync async, int count)
        {
           
            count--;
            if (count > 1)
            {
                var pk = await async.GetForEmit<IClientPack>().TestRecAsync(count);

                if (pk.IsError)
                    return 0;

                count = pk.As<int>();
            }
            return count;
        }

        [TAG(2501)]
        public async Task<int> TestRec2(IASync async, int count)
        {
            count--;
            if (count > 1)
            {
                var tmp = await async.GetForEmit<IClientPack>().TestRecAsync2(count);
                if (tmp.IsError)
                    return 0;

                count = tmp.As<int>();
            }

            return count;
        }
    }
}
