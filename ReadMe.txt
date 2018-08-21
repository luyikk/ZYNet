ZYNET is a cross platform fiber based network framework.
He uses RPC+CMD access 
Excellent performance, cross platform, strong stability 

Attention when using: IOS platform is unable to use Emit, so it also means that the RPC way can not be used, the only use of CMD, in detail, please see DEMO 

内核 socket tcp socketasyncevent 框架
通讯协议 protobuff
调用模式 interface + method or cmd+method
实现方法 await+fiber
是否跨平台 YES
跨平台实现方式 .net standard
是否支持xamarin 和 unity YES
稳定性 高
性能 高
可控性 高
可维护性 高
开发效率和可读性 高
框架大小 轻型
是否开源 YES
小白是否容易使用 YES


最低.NET 版本支持 .net 4.5
最低.net standard 支持 .net standard 1.3

使用方法:
Install-Package ZYNet

Xamarin for IOS:
Install-Package ZYNet-Portable

github:https://github.com/luyikk/ZYNet


使用方法:
打开VS 选择NUGIT 包管理 选择搜索 输入 ZYNET 选择相应的 需要的库
编译方法:
.NET 4.5 直接选择 ZYNET.sln 打开编译 输入DLL 在DLL 文件夹
.net standard 打开 netcore 选择 NetCore.sln 编译 dll在相应的bin目录下