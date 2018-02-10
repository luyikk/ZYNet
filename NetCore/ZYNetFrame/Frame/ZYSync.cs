using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;

namespace ZYNet.CloudSystem.Frame
{
    public class ZYSync : ZYSyncBase
    {
        public Action<byte[]> SyncSend { get; set; }

        public Func<long, byte[], Result> SyncSendAsWait { get; set; }

    
        public ZYSync()
        {
          
        }

#if !Xamarin
        public T Get<T>()
        {
            var tmp = DispatchProxy.Create<T, SyncProxy>();
            var proxy = tmp as SyncProxy;
            proxy.Call = Call;
            return tmp;
        }


        protected virtual object Call(MethodInfo method, object[] args)
        {
           
            var attr = method.GetCustomAttribute(typeof(TAG),true);

            if(attr==null)
            {
                throw new FormatException(method.Name + " Is Not MethodRun Attribute");
            }
            
            if (attr is TAG run)
            {
                int cmd= run.CmdTag;

                if (method.ReturnType != typeof(void))
                {

                    if (!Common.IsTypeOfBaseTypeIs(method.ReturnType, typeof(FiberThreadAwaiterBase)))
                    {
                        if (method.ReturnType == typeof(Result))
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
                        throw new Exception("Sync Call Not Use Return Type Of ResultAwatier");
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

#endif


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

      

        
        protected override Result SendDataAsWait(long Id, byte[] Data)
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
