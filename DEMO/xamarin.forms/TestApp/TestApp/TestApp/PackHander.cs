using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.Frame;
using System.Net.Http;
using Autofac;

namespace ZYNETClientForNetCore
{

    /// <summary>
    /// 客户端包处理器
    /// </summary>   
    public class PackHander
    {        

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
        public async Task<int> TestRec(IASync async, int count)
        {
            count--;
            if (count > 1)
                count = (await async.Get<IPacker>().TestRecAsync(count)).As<int>();
            return count;
        }
    }
}
