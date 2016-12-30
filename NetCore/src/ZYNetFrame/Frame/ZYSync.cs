using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;
using ZYSocket.ZYCoroutinesin;

namespace ZYNet.CloudSystem.Frame
{
    public class ZYSync : ZYSyncBase
    {
        public Action<byte[]> SyncSend { get; set; }

        public Func<long, byte[], ReturnResult> SyncSendAsWait { get; set; }

    
        public ZYSync()
        {
          
        }

        public T Get<T>()
        {
            var tmp = DispatchProxy.Create<T, SyncProxy>();
            var proxy = tmp as SyncProxy;
            proxy.Call = Call;
            return tmp;
        }


        protected virtual object Call(MethodInfo method, object[] args)
        {
           
            var attr = method.GetCustomAttribute(typeof(MethodRun),true);

            if(attr==null)
            {
                throw new FormatException(method.Name + " Is Not MethodRun Attribute");
            }
            

            MethodRun run = attr as MethodRun;

            if (run != null)
            {
                int cmd= run.CmdType;

                if (method.ReturnType != typeof(void))
                {
#if !COREFX
                    if (method.ReturnType.BaseType != typeof(FiberThreadAwaiterBase))
#else
                    if (method.ReturnType.GetTypeInfo().BaseType != typeof(FiberThreadAwaiterBase))
#endif
                    {
                        if (method.ReturnType == typeof(ReturnResult))
                        {
                            return CR(cmd, args);
                        }
                        else
                        {
                            try
                            {
                                return CR(cmd, args)?.First?.Value(method.ReturnType);
                            }
                            catch (Exception er)
                            {
                                throw new Exception(string.Format("Return Type of {0} Error", method.ReturnType), er);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Sync Call Not Use Return Type Of FiberThreadAwaiter<ReturnResult>");
                    }
                }
                else
                {
                    CV(cmd, args);

                    return null;
                }               

            }
            else
                return null;
        }






        protected override void SendData(byte[] Data)
        {
            if(SyncSend!=null)
            {
                SyncSend(Data);
            }
            else
            {
                throw new NullReferenceException("SyncSend Not Reg");
            }
        }

      

        
        protected override ReturnResult SendDataAsWait(long Id, byte[] Data)
        {
            if (SyncSend != null)
            {
                return SyncSendAsWait(Id, Data);
            }
            else
                throw new NullReferenceException("SyncSendAsWait Not Reg");
        }
    }
}
