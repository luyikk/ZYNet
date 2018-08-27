using System;
using System.Collections.Generic;
using System.Text;

namespace ZYNet.CloudSystem.Server.Options
{
    /// <summary>
    /// buffer size options
    /// </summary>
    public class BufferSizeOptions
    {
        /// <summary>
        /// 每个SOCKET连接的最大缓冲区BUFF 默认128K
        /// </summary>
        public int MaxBufferSize { get; set; } = 128 * 1024;
        /// <summary>
        /// 最大可接受多少大的数据包(轮盘缓冲区大小) 默认1M
        /// </summary>
        public int MaxPackSize { get; set; } = 1024 * 1024;

        /// <summary>
        /// 数据包每次发送大小,大于此大小将切片 默认不切片 -1
        /// </summary>
        public int SendBufferSize { get; set; } = -1;
    }
}
