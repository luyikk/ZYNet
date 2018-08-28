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
