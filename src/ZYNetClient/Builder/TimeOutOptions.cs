using System;
using System.Collections.Generic;
using System.Text;

namespace ZYNet.CloudSystem.Client.Options
{
    public class TimeOutOptions
    {
        /// <summary>
        /// 请求超时时间 (Server.GetData() throw timeoutexception) 默认1分钟
        /// </summary>
        public int MillisecondsTimeout { get; set; } = 60000;

        /// <summary>
        /// 是否检查求情超时,如果为false,那么将永久等待Server返回数据并且MillisecondsTimeout是无效的,默认为false
        /// </summary>
        public bool IsCheckAsyncTimeOut { get; set; } = false;
    }
}
