using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZYNet.CloudSystem.Frame
{
    public abstract class AsyncRunBase
    {
        protected FiberThreadAwaiter<ReturnResult> awaiter;

        public long Id { get; protected set; }

        public abstract void CV(int cmdTag, params object[] args);

        public abstract FiberThreadAwaiter<ReturnResult> CR(int cmdTag, params object[] args);
       

        public void SetRet(ReturnResult result)
        {
            if (awaiter == null)
                awaiter = new FiberThreadAwaiter<ReturnResult>();

            awaiter.SetResult(result);
            awaiter.IsCompleted = true;
            if(awaiter.Continuation!=null)
                awaiter.Continuation();
        }

        public ReturnResult RET(params object[] args)
        {
            ReturnResult tmp = new ReturnResult(args);
            tmp.Id = this.Id;
            return tmp;
        }
    }
}
