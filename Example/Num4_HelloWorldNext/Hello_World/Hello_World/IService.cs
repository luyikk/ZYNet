using System;
using System.Collections.Generic;
using System.Text;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Frame;

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
        ResultAwatier ServerShowMsg2(string msg);


        /// <summary>
        /// 输入一个数字,让它递归到1返回
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        [TAG(3)]
        ResultAwatier NumberToOne(int a);


        /// <summary>
        /// 输入一个数字,让它递归到1返回的异步方法
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        [TAG(3)]
        ResultAwatier NumberRec(int num);

        [TAG(4)]
        ResultAwatier AddOne(int num);

        [TAG(5)]
        void Addit(int a);

        [TAG(6)]
        ResultAwatier Getit();
    }
}
