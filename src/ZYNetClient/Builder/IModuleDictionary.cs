using System.Collections.Generic;

namespace ZYNet.CloudSystem.Client
{
    public interface IModuleDictionary
    {
        Dictionary<int, IAsyncMethodDef> ModuleDiy { get; }

        void Install(object o);
    }
}