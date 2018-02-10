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
        protected ResultAwatier awaiter;

        public long Id { get; protected set; }

        public abstract void Action(int cmdTag, params object[] args);

        public abstract ResultAwatier Func(int cmdTag, params object[] args);
       

        public void SetRet(Result result)
        {
            if (awaiter != null)
            {

                awaiter.SetResult(result);
                awaiter.IsCompleted = true;

                Action _Continuation = awaiter.Continuation;
                awaiter = null;
                _Continuation?.Invoke();
            }
        }

        public Result RET(params object[] args)
        {
            Result tmp = new Result(args)
            {
                Id = this.Id
            };
            return tmp;
        }
    }
}
