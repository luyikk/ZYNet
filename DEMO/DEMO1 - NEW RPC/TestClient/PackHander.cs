using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.Frame;
using System.Net;

namespace TestClient
{
    /// <summary>
    /// 客户端包处理器
    /// </summary>
    public class PackHander
    {
        [TAG(2001)]
        public async Task<ReturnResult> DownHtml(AsyncCalls async,string url)
        {
            WebClient client = new WebClient();
            byte[] html=  await client.DownloadDataTaskAsync(url);        

            return async.RET(html);
        }

        [TAG(3001)]
        public async Task Message(AsyncCalls async,string msg)
        {
            msg += (await async.Get<IPacker>().GetTimeAsync())?.First?.Value<DateTime>() ;
        
            Console.WriteLine(msg);
        }


        [TAG(2500)]
        public  async Task<ReturnResult> TestRec(AsyncCalls async, int count)
        {
            count--;
            if (count > 1)
            {
                var Packer = async.Get<IPacker>();

                var x = (await Packer.TestRecAsync(count))?[0]?.Value<int>();                

                if (x != null && x.HasValue)
                {
                    count = x.Value;
                }
            }

            return async.RET(count);
        }
    }
}
