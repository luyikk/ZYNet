using Autofac;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;


namespace ZYNet.CloudSystem.Client
{
    public interface IASync
    {
        IContainer Container { get; }

        CloudClient Client { get; }

        bool IsASync { get; }

        ZYSync Sync { get; }

        T Get<T>();
#if !Xamarin
        T GetForEmit<T>();
#endif
        void Action(int cmdTag, params object[] args);

        ResultAwatier Func(int cmdTag, params object[] args);

        Result Res(params object[] args);

        Task<Result> ResTask(params object[] args);


    }
}
