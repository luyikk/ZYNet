using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Interfaces;

namespace ZYNet.CloudSystem.Server
{
    public abstract class CloudRegister : CloudBase
    {

        public Dictionary<int, IAsyncMethodDef> CallsMethods { get; protected set; }

        public void Install(Type packHandlerType)
        {

            if (packHandlerType.BaseType != typeof(ControllerBase))
                throw new TypeLoadException($"{packHandlerType.Name} not inherit ControllerBase");

            var methods = packHandlerType.GetMethods();


            Type tasktype = typeof(Task);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttributes(typeof(TAG), true);


                foreach (var att in attr)
                {
                    if (att is TAG attrcmdtype)
                    {

                        if (!CallsMethods.ContainsKey(attrcmdtype.CmdTag))
                        {                            
                            IAsyncMethodDef tmp = base.Container.Resolve<IAsyncMethodDef>(new NamedParameter("implementationType", packHandlerType), new NamedParameter("methodInfo", method));

                            if (method.GetParameters().Length == 0 || (method.GetParameters()[0].ParameterType.BaseType!=typeof(IASync) && method.GetParameters()[0].ParameterType != typeof(IASync)))
                                tmp.IsNotAsyncArg = true;

                            CallsMethods.Add(attrcmdtype.CmdTag, tmp);
                        }

                        break;
                    }

                }

            }
           
        }

        public void Install(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if(type.IsClass&&type.BaseType==typeof(ControllerBase))
                {
                    Install(type);
                }
            } 
        }

    }
}
