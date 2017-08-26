using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;

namespace ZYNet.CloudSystem.Client
{

    public class ModuleDictionary
    {
        public Dictionary<int, AsyncMethodDef> ModuleDiy { get; private set; }


        public ModuleDictionary()
        {
            ModuleDiy = new Dictionary<int, AsyncMethodDef>();
        }

        public void Install(object o)
        {
            Type type = o.GetType();

            var methos = type.GetMethods();

            Type tasktype = typeof(Task);


            foreach (var method in methos)
            {

                var attr = method.GetCustomAttributes(typeof(MethodCmdTag), true);

                foreach (var att in attr)
                {

                    MethodCmdTag attrcmdtype = att as MethodCmdTag;

                    if (attrcmdtype != null)
                    {

                        if ((method.ReturnType == tasktype || (Common.IsTypeOfBaseTypeIs(method.ReturnType,tasktype) && method.ReturnType.IsConstructedGenericType && method.ReturnType.GenericTypeArguments[0] == typeof(ReturnResult))))
                        {

                            if (method.GetParameters().Length > 0 && method.GetParameters()[0].ParameterType == typeof(AsyncCalls))
                            {
                                if (!ModuleDiy.ContainsKey(attrcmdtype.CmdTag))
                                {
                                    AsyncMethodDef tmp = new AsyncMethodDef(method, o);
                                    ModuleDiy.Add(attrcmdtype.CmdTag, tmp);
                                }
                                else
                                {
                                    AsyncMethodDef tmp = new AsyncMethodDef(method, o);
                                    ModuleDiy[attrcmdtype.CmdTag] = tmp;
                                }
                            }

                        }
                        else if (method.GetParameters().Length > 0 && method.GetParameters()[0].ParameterType == typeof(CloudClient))
                        {
                            if (!ModuleDiy.ContainsKey(attrcmdtype.CmdTag))
                            {
                                AsyncMethodDef tmp = new AsyncMethodDef(method, o);
                                ModuleDiy.Add(attrcmdtype.CmdTag, tmp);
                            }
                            else
                            {
                                AsyncMethodDef tmp = new AsyncMethodDef(method, o);
                                ModuleDiy[attrcmdtype.CmdTag] = tmp;
                            }
                        }
                        

                        break;
                    }
                }
            }

        }

    }

    public class AsyncMethodDef
    {
        static Type tasktype = typeof(Task);

        public object Obj { get; private set; }

        public bool IsAsync { get; set; }
        public bool IsOut { get; set; }

        public MethodInfo methodInfo { get; set; }

        public Type[] ArgsType { get; set; }

        

        public AsyncMethodDef(MethodInfo methodInfo, object token)
        {
            this.Obj = token;
            this.methodInfo = methodInfo;

            var parameters = methodInfo.GetParameters();
            ArgsType = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ArgsType[i] = parameters[i].ParameterType;
            }
                      

            if (methodInfo.ReturnType == tasktype || methodInfo.ReturnType == null|| methodInfo.ReturnType == typeof(void))
            {
                IsOut = false;
            }
            else
                IsOut = true;


            if (Common.IsTypeOfBaseTypeIs(methodInfo.ReturnType ,tasktype))
            {
                IsAsync = true;
            }
        }


    }
}
