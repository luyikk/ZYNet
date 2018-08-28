using System;
using System.Collections.Generic;
using System.Text;

namespace ZYNet.CloudSystem.Client.Options
{
    public class DataEncodeOptions
    {
        /// <summary>
        /// 数据包解密
        /// </summary>
        public Func<byte[], byte[]> DecodeingHandler { get; set; } = null;

        /// <summary>
        /// 数据包加密
        /// </summary>

        public Func<byte[], byte[]> EcodeingHandler { get; set; } = null;
    }
}
