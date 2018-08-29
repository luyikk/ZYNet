using System.Threading.Tasks;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Frame;

namespace Hello_World_Server
{
    /// <summary>
    /// 在服务器上添加一个接口,用来访问客户端,结构和客户端类似
    /// </summary>
    [Build]
    public interface IClientServ
    {
        //再次申明我们只要知道TAG =几就可以了,参数是啥,返回类型是啥, 只要接口名么,随便啦,TAG 是最重要的,记住记住
        //返回类型必须是 ResultAwatier,其他的不行,也许你会想,客户端返回类型明明是bool这里ResultAwatier能可以吗?
        //其实 ZYNET 返回值定义了一个Result类型,他可以保存任何可序列化对象,而且可以保存多个返回对象,这样就实现了OUT X,OUT Y,
        //ResultAwatier就是 await的Result版本,你可以这么理解
        [TAG(101)]
        ResultAwatier ShowMsg(string msg);

        /// <summary>
        /// 表扬接口
        /// </summary>
        /// <param name="msg"></param>
        [TAG(102)]
        void Good(string msg);
    }
}
