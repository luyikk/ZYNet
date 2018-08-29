using System;
using System.Collections.Generic;
using System.Text;
using ZYNet.CloudSystem;

namespace Hello_World
{
    /// <summary>
    /// 7创建一个接口让它看起来像这样
    /// </summary>

    [Build]
    public interface IService
    {
        /// <summary>
        /// 让服务器显示Msg
        /// </summary>
        /// <param name="msg"></param>
        [TAG(1)]
        void ServerShowMsg(string msg);

    }
}
