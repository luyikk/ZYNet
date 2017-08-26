using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.Frame;
using System.Net.Http;

namespace ZYNETClientForNetCore
{

    /// <summary>
    /// 客户端包处理器
    /// </summary>
    public class PackHander
    {
        [MethodCmdTag(2001)]
        public async Task<ReturnResult> DownHtml(AsyncCalls async, string url)
        {
            HttpClient  client = new HttpClient();
            byte[] html = await client.GetByteArrayAsync(url);

            return async.RET(html);
        }

        [MethodCmdTag(3001)]
        public void Message(CloudClient client, string msg)
        {
            Console.WriteLine(msg);
        }


        [MethodCmdTag(2500)]
        public async Task<ReturnResult> TestRec(AsyncCalls async, int count)
        {
            count--;
            if (count > 1)
            {
                var x = (await async.CR(2500, count))?[0]?.Value<int>();

                if (x != null && x.HasValue)
                {
                    count = x.Value;
                }
            }

            return async.RET(count);
        }
    }
}
