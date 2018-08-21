using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Frame;

namespace ZYNet.CloudSystem.Client
{
    public class ZYSyncClient:ZYSync, IFodyCall
    {



        private Dictionary<Type, Type> FodyDir { get; set; }

        public ZYSyncClient():base()
        {
            FodyDir = new Dictionary<Type, Type>();
        }

        /// <summary>
        /// Need Nuget Install-Package Fody
        /// And Add xml file 'FodyWeavers.xml' to project
        /// context:
        /// \<?xml version="1.0" encoding="utf-8" ?\>
        /// \<Weavers\>
        /// \<Virtuosity\/\> 
        /// \</Weavers\>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() 
        {
            var interfaceType = typeof(T);
            if (!FodyDir.ContainsKey(interfaceType))
            {
                var assembly = interfaceType.Assembly;
                var implementationType = assembly.GetType(interfaceType.FullName + "_Builder_Implementation");
                if (implementationType == null)
                    throw new Exception("not find with {interfaceType.FullName} the Implementation");
                FodyDir.Add(interfaceType, implementationType);
                return (T)Activator.CreateInstance(implementationType, new Func<int, Type, object[], object>(Call));

            }
            else
            {
                return (T)Activator.CreateInstance(FodyDir[interfaceType], new Func<int, Type, object[], object>(Call));

            }

        }

        public virtual object Call(int cmd, Type returnType, object[] args)
        {
            
            if (returnType != typeof(void))
            {

                if (!Common.IsTypeOfBaseTypeIs(returnType, typeof(FiberThreadAwaiterBase)))
                {
                    if (returnType == typeof(Result))
                    {
                        return Func(cmd, args);
                    }
                    else
                    {
                        try
                        {
                            return Func(cmd, args)?.First?.Value(returnType);
                        }
                        catch (Exception er)
                        {
                            throw new Exception(string.Format("Return Type of {0} Error", returnType), er);
                        }
                    }
                }
                else
                {
                    throw new Exception("Sync Call Not Use Return Type Of ResultAwatier");
                }
            }
            else
            {
                Action(cmd, args);

                return null;
            }


        }


#if !Xamarin
        public T GetForEmit<T>()
        {
            var tmp = DispatchProxy.Create<T, SyncProxy>();
            var proxy = tmp as SyncProxy;
            proxy.Call = Call;
            return tmp;
        }


        protected virtual object Call(MethodInfo method, object[] args)
        {

            var attr = method.GetCustomAttribute(typeof(TAG), true);

            if (attr == null)
            {
                throw new FormatException(method.Name + " Is Not MethodRun Attribute");
            }

            if (attr is TAG run)
            {
                int cmd = run.CmdTag;

                if (method.ReturnType != typeof(void))
                {

                    if (!Common.IsTypeOfBaseTypeIs(method.ReturnType, typeof(FiberThreadAwaiterBase)))
                    {
                        if (method.ReturnType == typeof(Result))
                        {
                            return Func(cmd, args);
                        }
                        else
                        {
                            try
                            {
                                return Func(cmd, args)?.First?.Value(method.ReturnType);
                            }
                            catch (Exception er)
                            {
                                throw new Exception(string.Format("Return Type of {0} Error", method.ReturnType), er);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Sync Call Not Use Return Type Of ResultAwatier");
                    }
                }
                else
                {
                    Action(cmd, args);

                    return null;
                }

            }
            else
                return null;
        }

#endif
    }
}
