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

    public delegate void LogOutHandler(string msg, LogType type);

    public static class LogAction
    {
        public static event LogOutHandler LogOut;
        

        public static void Warn(string msg, params object[] args)
        {
            LogOut?.Invoke(string.Format(msg, args), LogType.War);
        }

        public static void Err(string msg, params object[] args)
        {
            LogOut?.Invoke(string.Format(msg, args), LogType.Err);
        }

        public static void Log(string msg, params object[] args)
        {
            LogOut?.Invoke(string.Format(msg, args), LogType.Log);
        }

        public static void Log(LogType type,string msg,params object[] args)
        {
            LogOut?.Invoke(string.Format(msg, args), type);
        }
    }
}
