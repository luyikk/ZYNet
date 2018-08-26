using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;

namespace ZYNet.CloudSystem.Server
{
    public class AsyncMethodDef
    {
        static  Type tasktype = typeof(Task);

        public bool IsAsync { get; set; }

        public bool IsRet { get; set; }

        public Type ImplementationType { get; set; }

        public MethodInfo MethodInfo { get; set; }

        public Type[] ArgsType { get; set; }



        public AsyncMethodDef(Type implementationType,MethodInfo methodInfo)
        {

          
            this.MethodInfo = methodInfo;
            this.ImplementationType = implementationType;
            var parameters = methodInfo.GetParameters();
            ArgsType = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ArgsType[i] = parameters[i].ParameterType;
            }

            if (methodInfo.ReturnType == tasktype || methodInfo.ReturnType == null||methodInfo.ReturnType==typeof(void))
            {
                IsRet = false;
            }
            else
                IsRet = true;


            if (Common.IsTypeOfBaseTypeIs(methodInfo.ReturnType,tasktype))
            {
                IsAsync = true;
            }
        }
    }
}
