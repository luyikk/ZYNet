using System;
using System.Collections.Generic;
using System.Text;

namespace ZYNet.CloudSystem.Server.Options
{
    /// <summary>
    /// From here you can control the maximum number of connections
    /// </summary>
    public class MaxConnectNumberOptions
    {
        /// <summary>
        /// default is 10000;
        /// </summary>
        public int MaxConnNumber { get; set; } = 10000;
    }
}
