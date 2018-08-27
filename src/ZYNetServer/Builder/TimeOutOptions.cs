using System;
using System.Collections.Generic;
using System.Text;

namespace ZYNet.CloudSystem.Server.Options
{
    /// <summary>
    /// 超时设置
    /// </summary>
    public class TimeOutOptions
    {
        /// <summary>
        ///TOKEN清理时间,默认30秒,断线30秒内重新连接将保留用户临时数据
        /// </summary>
        public TimeSpan TokenWaitClecrTime { get; set; } = TimeSpan.FromSeconds(30);


        /// <summary>
        /// 是否检查求情超时,如果为false,那么将永久等待Client返回数据并且ReadOutTimeMilliseconds是无效的,默认为false
        /// </summary>
        public bool IsCheckReadOutTime { get; set; } = false;

        /// <summary>
        /// 请求超时时间 (await client.GetData() throw timeoutexception) 默认1分钟
        /// </summary>
        public int ReadOutTimeMilliseconds { get; set; } = 60000;

     

    }
}
