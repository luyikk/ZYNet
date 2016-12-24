using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ZYNet.CloudSystem.Server
{
    public class AsyncStaticMethodDef
    {
        static  Type tasktype = typeof(Task);
        public bool IsAsync { get; set; }

        public bool IsOut { get; set; }

        public MethodInfo methodInfo { get; set; }

        public Type[] ArgsType { get; set; }



        public AsyncStaticMethodDef(MethodInfo methodInfo)
        {

          
            this.methodInfo = methodInfo;
            var parameters = methodInfo.GetParameters();
            ArgsType = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ArgsType[i] = parameters[i].ParameterType;
            }

            if (methodInfo.ReturnType == tasktype || methodInfo.ReturnType == null||methodInfo.ReturnType==typeof(void))
            {
                IsOut = false;
            }
            else
                IsOut = true;


#if !COREFX
            if (methodInfo.ReturnType == tasktype || methodInfo.ReturnType.BaseType==tasktype)
#else
            if (methodInfo.ReturnType == tasktype || methodInfo.ReturnType.GetTypeInfo().BaseType == tasktype)
#endif
            {
                IsAsync = true;
            }
        }
    }
}
