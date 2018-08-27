using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ZYNet.CloudSystem.Server
{
    public abstract class InstallController
    {

        public Dictionary<int, AsyncMethodDef> CallsMethods { get; protected set; }

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
                            AsyncMethodDef tmp = new AsyncMethodDef(packHandlerType, method);
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
