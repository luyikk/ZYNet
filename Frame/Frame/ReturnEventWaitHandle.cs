using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Loggine;

namespace ZYNet.CloudSystem.Frame
{
    public class ReturnEventWaitHandle:IDisposable
    {
        protected static readonly ILog Log = LogFactory.ForContext<ReturnEventWaitHandle>();

        public Result Result { get; private set; }

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
                Log.Warn("Wait Result Time Out");
               
                return false;
            }
            return true;
        }

        public void Set(Result result)
        {
            this.Result = result;
            if (!IsClose)
            {
                WaitHandle.Set();
            }
        }

        public void Dispose()
        {
            WaitHandle.Dispose();
            IsClose = true;
        }
    }
}
