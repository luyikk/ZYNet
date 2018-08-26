using System;
using System.Collections.Generic;
using System.Text;
using ZYNet.CloudSystem.Frame;

namespace ZYNet.CloudSystem.Frame
{
    public abstract class MessageExceptionParse
    {
        public  Func<Exception,bool> ExceptionOut { get; set; }


        public bool PushException(Exception er)
        {           
            var bp= ExceptionOut?.Invoke(er);

            if (bp == null)
                return true;
            else
                return bp.Value;
        }


        protected Result GetExceptionResult(Exception er)
        {
            var r = new Result()
            {
                ErrorMsg = er.ToString(),
                ErrorId = er.HResult
            };

          
            return r;

        }

        protected Result GetExceptionResult(Exception er, long id)
        {
            var r = new Result()
            {
                ErrorMsg = er.ToString(),
                ErrorId = er.HResult,
                Id = id
            };

         

            return r;

        }

        protected Result GetExceptionResult(Exception er, int errorid)
        {
            var r = new Result()
            {
                ErrorMsg = er.ToString(),
                ErrorId = errorid

            };

          
            return r;

        }

        protected Result GetExceptionResult(Exception er, long id, int errorid)
        {
            var r = new Result()
            {
                ErrorMsg = er.ToString(),
                ErrorId = errorid,
                Id = id

            };

          
            return r;

        }




        protected Result GetExceptionResult(object er, int errorid, long id)
        {
            var r = new Result()
            {
                ErrorMsg = er.ToString(),
                ErrorId = errorid,
                Id = id
            };

            return r;

        }

        protected Result GetExceptionResult(object er, int errorid)
        {
            var r = new Result()
            {
                ErrorMsg = er.ToString(),
                ErrorId = errorid

            };
            return r;

        }


    }

    public class ReturnTypeException:Exception
    {
        public ReturnTypeException(string msg,int erroid):base(msg)
        {
            base.HResult = erroid;
        }

        public ReturnTypeException(string msg, int erroid, Exception ers) : base(msg,ers)
        {
            base.HResult = erroid;
        }
    }

    public class CallException : Exception
    {
        public CallException(string msg, int erroid) : base(msg)
        {
            base.HResult = erroid;
        }

        public CallException(string msg, int erroid, Exception ers) : base(msg, ers)
        {
            base.HResult = erroid;
        }
    }

    public class SetResultException : Exception
    {
        public SetResultException(string msg, int erroid) : base(msg)
        {
            base.HResult = erroid;
        }

        public SetResultException(string msg, int erroid, Exception ers) : base(msg, ers)
        {
            base.HResult = erroid;
        }
    }

    public class FodyInstallException : Exception
    {
        public FodyInstallException(string msg, int erroid) : base(msg)
        {
            base.HResult = erroid;
        }

        public FodyInstallException(string msg, int erroid, Exception ers) : base(msg, ers)
        {
            base.HResult = erroid;
        }
    }
}
