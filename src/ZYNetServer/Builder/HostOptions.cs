using System;
using System.Collections.Generic;
using System.Text;

namespace ZYNet.CloudSystem.Server.Options
{
    /// <summary>
    /// 服务IP 和端口 设置 默认HOST 绑定所有IP地址 默认端口10001
    /// </summary>
    public class HostOptions
    {

        public string Host { get; set; } = "any";

        public int Port { get; set; } = 10001;

    }
}
