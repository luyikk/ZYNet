using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;

namespace ZYNet.CloudSystem.Client
{

    public class ModuleDictionary : IModuleDictionary
    {
        public Dictionary<int, IAsyncMethodDef> ModuleDiy { get; private set; }


        public ModuleDictionary()
        {
            ModuleDiy = new Dictionary<int, IAsyncMethodDef>();
        }

        public void Install(object o)
        {
            Type type = o.GetType();

            var methos = type.GetMethods();

            Type tasktype = typeof(Task);


            foreach (var method in methos)
            {

                var attr = method.GetCustomAttributes(typeof(TAG), true);

                foreach (var att in attr)
                {


                    if (att is TAG attrcmdtype)
                    {

                        if ((method.ReturnType == tasktype || (Common.IsTypeOfBaseTypeIs(method.ReturnType, tasktype) && method.ReturnType.IsConstructedGenericType)))
                        {

                            if (method.GetParameters().Length > 0 && (method.GetParameters()[0].ParameterType == typeof(AsyncCalls)|| method.GetParameters()[0].ParameterType == typeof(IASync)))
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
                            else if(method.GetParameters().Length==0|| (method.GetParameters()[0].ParameterType!= typeof(AsyncCalls) && method.GetParameters()[0].ParameterType != typeof(IASync)))
                            {
                                if (!ModuleDiy.ContainsKey(attrcmdtype.CmdTag))
                                {
                                    AsyncMethodDef tmp = new AsyncMethodDef(method, o)
                                    {
                                        IsNotRefAsyncArg = true
                                    };
                                    
                                    ModuleDiy.Add(attrcmdtype.CmdTag, tmp);
                                }
                                else
                                {
                                    AsyncMethodDef tmp = new AsyncMethodDef(method, o)
                                    {
                                        IsNotRefAsyncArg = true
                                    };
                                    ModuleDiy[attrcmdtype.CmdTag] = tmp;
                                }
                            }

                        }
                        else if (method.GetParameters().Length > 0 && (method.GetParameters()[0].ParameterType == typeof(CloudClient) || method.GetParameters()[0].ParameterType == typeof(IASync)))
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
                        else if (method.GetParameters().Length == 0 || (method.GetParameters()[0].ParameterType != typeof(CloudClient) || method.GetParameters()[0].ParameterType != typeof(IASync)))
                        {
                            if (!ModuleDiy.ContainsKey(attrcmdtype.CmdTag))
                            {
                                AsyncMethodDef tmp = new AsyncMethodDef(method, o)
                                {
                                    IsNotRefAsyncArg = true
                                };
                                ModuleDiy.Add(attrcmdtype.CmdTag, tmp);
                            }
                            else
                            {
                                AsyncMethodDef tmp = new AsyncMethodDef(method, o)
                                {
                                    IsNotRefAsyncArg = true
                                };
                                ModuleDiy[attrcmdtype.CmdTag] = tmp;
                            }
                        }


                        break;
                    }
                }
            }

        }

    }

    public class AsyncMethodDef : IAsyncMethodDef
    {
            Type tasktype = typeof(Task);

        public object Obj { get; private set; }

        public bool IsAsync { get; set; }
        public bool IsRet { get; set; }

        public bool IsNotRefAsyncArg { get; set; }

        public MethodInfo MethodInfo { get; set; }

        public Type[] ArgsType { get; set; }

        public AsyncMethodDef(MethodInfo methodInfo, object token)
        {
            this.Obj = token;
            this.MethodInfo = methodInfo;

            var parameters = methodInfo.GetParameters();
            ArgsType = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ArgsType[i] = parameters[i].ParameterType;
            }
                      

            if (methodInfo.ReturnType == tasktype || methodInfo.ReturnType == null|| methodInfo.ReturnType == typeof(void))
            {
                IsRet = false;
            }
            else
                IsRet = true;


            if (Common.IsTypeOfBaseTypeIs(methodInfo.ReturnType ,tasktype))
            {
                IsAsync = true;
            }
        }


    }
}
