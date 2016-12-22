using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZYNet.CloudSystem
{
   

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MethodRun : Attribute
    {
        public int CmdType { get; set; }

        /// <summary>
        /// 数据包格式化类
        /// </summary>
        /// <param name="bufferCmdType">数据包命令类型</param>
        public MethodRun(int bufferCmdType)
        {
            this.CmdType = bufferCmdType;
        }
    }

}
