using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZYNet.CloudSystem
{
   

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class MethodCmdTag : Attribute
    {
        public int CmdTag { get; set; }

        /// <summary>
        /// 数据包格式化类
        /// </summary>
        /// <param name="bufferCmdTag">数据包命令类型</param>
        public MethodCmdTag(int bufferCmdTag)
        {
            this.CmdTag = bufferCmdTag;
        }
    }

}
