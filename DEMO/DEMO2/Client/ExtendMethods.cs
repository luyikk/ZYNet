using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.Frame;
namespace Client
{
    public enum Cmdtype
    {
        LogOn=1000,
        SendAll=2001,
        SendTo= 2002,
    }


    public static class ExtendMethods
    {
        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="sync"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public static ReturnResult LogOn(this ZYSync sync,string username)
        {
            return sync.CR((int)Cmdtype.LogOn, username);
        }

        public static void SendMessageToAllUser(this ZYSync sync,string msg)
        {
            sync.CV((int)Cmdtype.SendAll, msg);
        }
        
        public static string SendMsgToUser(this ZYSync sync,string account, string msg)
        {
            return sync.CR((int)Cmdtype.SendTo,account,msg)?.First?.Value<string>();
        }

    }
}
