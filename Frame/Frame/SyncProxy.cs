using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ZYNet.CloudSystem.Frame
{
    public class SyncProxy : DispatchProxy
    {
        public  Func<MethodInfo, object[], object> Call { get; set; }


        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (Call != null)
                return Call(targetMethod, args);
            else
                return null;
        }
    }
}
