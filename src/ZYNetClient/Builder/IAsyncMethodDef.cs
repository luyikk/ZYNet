using System;
using System.Reflection;

namespace ZYNet.CloudSystem.Client
{
    public interface IAsyncMethodDef
    {
        Type[] ArgsType { get; set; }
      
        bool IsAsync { get; set; }
        bool IsNotRefAsyncArg { get; set; }
        bool IsRet { get; set; }
        MethodInfo MethodInfo { get; set; }
        object Obj { get; }
    }
}