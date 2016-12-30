using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZYNet.CloudSystem.Frame
{
    public class ReturnEventWaitHandle:IDisposable
    {
        public ReturnResult Result { get; private set; }

        public EventWaitHandle WaitHandle { get; }

        public int MillisecondsTimeout { get; set; }

        private bool IsClose;

        public ReturnEventWaitHandle(int millisecondsTimeout, bool initialState, EventResetMode mode)
        {
            this.MillisecondsTimeout = millisecondsTimeout;
           
            WaitHandle = new EventWaitHandle(initialState, mode);
        }



        public bool WaitOne()
        {
            if(!WaitHandle.WaitOne(MillisecondsTimeout))
            {
                LogAction.Log(LogType.War,"Wait Result Time Out");

            }
            return true;
        }

        public void Set(ReturnResult result)
        {
            this.Result = result;
            if (!IsClose)
            {
                WaitHandle.Set();
            }
        }

        public void Dispose()
        {
#if !COREFX
            WaitHandle.Close();
#endif
            WaitHandle.Dispose();
            IsClose = true;
        }
    }
}
