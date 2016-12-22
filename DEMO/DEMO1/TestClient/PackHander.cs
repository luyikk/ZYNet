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
        [MethodRun(2001)]
        public async Task<ReturnResult> DownHtml(AsyncCalls async,string url)
        {
            WebClient client = new WebClient();
            byte[] html=  await client.DownloadDataTaskAsync(url);

         

            return async.RET(html);
        }

        [MethodRun(3001)]
        public void Message(CloudClient client,string msg)
        {
            
            Console.WriteLine(msg);
        }
    }
}
