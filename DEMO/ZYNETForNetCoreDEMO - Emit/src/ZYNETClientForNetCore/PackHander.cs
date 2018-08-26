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
    public class PackHander: IController
    {
        public CloudClient CClient { get; set; }
        public AsyncCalls Async { get; set; }
        public bool IsAsync { get; set; }



        [TAG(2001)]
        public async Task<byte[]> DownHtml(string url)
        {
           
            HttpClient  client = new HttpClient();
            return await client.GetByteArrayAsync(url);          
        }

        [TAG(3001)]
        public void Message(string msg)
        {            
            Console.WriteLine(msg);
        }


        [TAG(2500)]
        public async Task<int> TestRec(AsyncCalls async, int count)
        {
            count--;
            if (count > 1)
            {               

                var x = (await async.Func(2500, count));

                if (x != null && x.IsHaveValue)
                {
                    count = x.As<int>(); ;
                }
            }

            return count;
        }


        [TAG(2501)]
        public async Task<int> TestRec2(int count)
        {
            count--;
            if (count > 1)
            {
                var x = (await Async.Func(2501, count))?[0]?.Value<int>();

                if (x != null && x.HasValue)
                {
                    count = x.Value;
                }
            }

            return count;
        }
    }
}
