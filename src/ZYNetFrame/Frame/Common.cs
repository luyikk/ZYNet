using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;

namespace ZYNet.CloudSystem.Frame
{
    public static class Common
    {
        public static object _lock = new object();

        public static long Id = 10000;

        public static long MakeID => Interlocked.Increment(ref Id);

        public static bool IsTypeOfBaseTypeIs(Type type,Type baseType)
        {
            if (type == baseType)
                return true;



            if (type.BaseType == null)
                return false;

            if (type.BaseType == baseType)
                return true;
            else
                return IsTypeOfBaseTypeIs(type.BaseType, baseType);


        }

    }
}
