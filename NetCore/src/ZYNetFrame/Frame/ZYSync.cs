using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;

namespace ZYNet.CloudSystem.Frame
{
    public class ZYSync : ZYSyncBase
    {
        public Action<byte[]> SyncSend { get; set; }

        public Func<long, byte[], ReturnResult> SyncSendAsWait { get; set; }

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
