using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Frame;


namespace TestClient
{
    /// <summary>
    /// 定义一个服务器接口用来调用服务器
    /// </summary>
    public interface IPacker
    {
        [MethodCmdTag(1000)]
        ReturnResult IsLogOn(string username, string password);

        [MethodCmdTag(2001)]
        ResultAwatier StartDownAsync(string url);

        [MethodCmdTag(2001)]
        ReturnResult StartDown(string url);



        /// <summary>
        /// 同步调用返回DateTime 注意：如果你写的CMD 服务器返回类型和此DateTime类型有出入或者无法返回，以及返回超时，此函数将抛出异常
        /// </summary>
        /// <returns></returns>
        [MethodCmdTag(2002)]
        DateTime GetTime();

        /// <summary>
        /// 同步调用返回DateTime 注意：此方法调用失败将 返回null 而不会发生异常
        /// </summary>
        /// <returns></returns>
        [MethodCmdTag(2002)]
        ReturnResult GetTimer();

        /// <summary>
        /// 异步调用版本，只能在异步数据包处理时才能使用，无法在主线程同步方法中使用
        /// </summary>
        /// <returns></returns>

        [MethodCmdTag(2002)]
        ResultAwatier GetTimeAsync();


        [MethodCmdTag(2003)]
        void SetPassWord(string password);

        [MethodCmdTag(2500)]
        ReturnResult TestRec(int count);

        [MethodCmdTag(2500)]
        ResultAwatier TestRecAsync(int count);

        [MethodCmdTag(2500)]
        int TestRec2(int count);



    }
}
