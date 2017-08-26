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
    /// 客户端逻辑处理器
    /// </summary>
    public class PackHander
    {
        /// <summary>
        /// 下载函数 留给服务器调用
        /// </summary>
        /// <param name="async"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        [MethodCmdTag(2001)]
        public async Task<ReturnResult> DownHtml(AsyncCalls async,string url)
        {
            WebClient client = new WebClient();
            byte[] html=  await client.DownloadDataTaskAsync(url);         
            return async.RET(html);
        }

        /// <summary>
        /// 消息打印
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        [MethodCmdTag(3001)]
        public void Message(CloudClient client,string msg)
        {            
            Console.WriteLine(msg);
        }

        /// <summary>
        /// 递归测试
        /// </summary>
        /// <param name="async"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [MethodCmdTag(2500)]
        public  async Task<ReturnResult> TestRec(AsyncCalls async, int count)
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
