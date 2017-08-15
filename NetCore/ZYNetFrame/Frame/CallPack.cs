using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZYNet.CloudSystem.Frame
{

    public static class CmdDef
    {
        public const int CallCmd = 0x1000;
        public const int ReturnResult = 0x1001;

    }

    
    public class CallPack
    {
        public long Id { get; set; }

        public int CmdTag { get; set; }

        public List<byte[]> Arguments { get; set; }
    }


    public class ResultValue
    {
        public byte[] Data { get; private set; }

        public ResultValue(byte[] data)
        {
            this.Data = data;
        }

        public T Value<T>()
        {
            return (T)Serialization.UnpackSingleObject(typeof(T), Data);
        }

        public object Value(Type type)
        {
            return Serialization.UnpackSingleObject(type, Data);
        }
    }


    public class ReturnResult
    {
        public long Id { get; set; }

        public List<byte[]> Arguments { get; set; }


        public ResultValue this[int index]
        {            
            get
            {
                if (index < Arguments.Count)
                {
                    return new ResultValue(Arguments[index]);
                }
                return null;
            }
        }

        public ResultValue First
        {
            get
            {
                if (Arguments == null || Arguments.Count == 0)
                    return null;
                
                return new ResultValue (Arguments[0]);
            }

        }

        public int? Length => Arguments?.Count;


        public ReturnResult()
        {

        }

        public ReturnResult(params object[] args)
        {
            if (args != null)
            {

                Arguments = new List<byte[]>(args.Length);

                foreach (var item in args)
                {
                    Arguments.Add(Serialization.PackSingleObject(item.GetType(), item));
                }
            }
        }


        public T As<T>(int index)
        {
           return (T)Serialization.UnpackSingleObject(typeof(T), Arguments[index]);
        }
        

    }

    public class ResultAwatier : FiberThreadAwaiter<ReturnResult>
    {
        public ResultAwatier(Fiber GhostThread):base(GhostThread)
        {

        }

        public ResultAwatier():base()
        {

        }

    }
    
   
}
