using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ZYNet.CloudSystem.Frame
{
    public static class Common
    {
        public static object _lock = new object();

        public static long Id = 10000;

        public static long MakeID => Interlocked.Increment(ref Id);
       
    }
}
