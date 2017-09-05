using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYNet.CloudSystem
{
    [Flags]
    public enum LogType
    {
        None=0,
        Log=1,
        Err=2,
        War = 3
    }

    public delegate void LogOutHandler(object sender, string msg, LogType type);

    public static class LogAction
    {
        public static event LogOutHandler LogOut;
        

        public static void Warn(object sender,string msg, params object[] args)
        {
            LogOut?.Invoke(sender,string.Format(msg, args), LogType.War);
        }

        public static void Err(object sender,string msg, params object[] args)
        {
            LogOut?.Invoke(sender,string.Format(msg, args), LogType.Err);
        }

        public static void Log(object sender,string msg, params object[] args)
        {
            LogOut?.Invoke(sender,string.Format(msg, args), LogType.Log);
        }

        public static void Log(object sender,LogType type,string msg,params object[] args)
        {
            LogOut?.Invoke(sender,string.Format(msg, args), type);
        }
    }
}
