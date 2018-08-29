using System;
using System.Collections.Generic;
using System.Text;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client;

namespace Hello_World
{
    /// <summary>
    /// 我们添加一个客户端的控制器类给服务器调用,这个类不需要继承啥啥哦
    /// </summary>
    public class ClientConroller
    {
              
        /// <summary>
        /// 显示MSG,显示完后告诉服务器我显示OK了
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [TAG(101)]
        public bool ShowMsg(string msg)
        { 
            Console.WriteLine(msg);
            return true;
        }

        [TAG(102)]
        public void MyGood(string msg)
        {
            Console.WriteLine($"我自豪:{msg}");
        }
    }
}
