using System;
using System.Collections.Generic;
using System.Text;
using ZYNet.CloudSystem;

namespace Hello_World
{
    /// <summary>
    /// 7创建一个接口让它看起来像这样
    /// </summary>

    [Build] //必须添加Build标签哦 GetForEmit<T>()方式除外 
    public interface IService
    {
        /// <summary>
        /// 让服务器显示Msg
        /// </summary>
        /// <param name="msg"></param>
        [TAG(1)]
        void ServerShowMsg(string msg);

        [TAG(2)] //主义这里的TAG,ZYNET使用TAG来分辨你调用的是哪个功能函数
        bool ServerShowMsg2(string msg);

    }
}
