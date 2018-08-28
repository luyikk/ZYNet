using System;
using System.Reflection;

namespace ZYNet.CloudSystem.Server
{
    public interface IAsyncMethodDef
    {
        Type[] ArgsType { get; set; }
        Type ImplementationType { get; set; }
        bool IsNotAsyncArg { get; set; }
        bool IsAsync { get; set; }
        bool IsRet { get; set; }
        MethodInfo MethodInfo { get; set; }
    }
}