﻿using System;
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
        public const int SetSession = 0x10FECEED;
    }

    
    public class CallPack
    {
        public long Id { get; set; }

        public int CmdTag { get; set; }

        public List<byte[]> Arguments { get; set; }
    }

   
    public class ZYNETException:Exception
    {
        public string ErrorMsg { get; set; }

        public int ErrorId { get; set; }

        public ZYNETException(string msg,int errorId)
        {
            this.ErrorMsg = msg;
            this.ErrorId = errorId;
        }


        public override string Message => ErrorMsg;
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


    public class Result
    {
        public long Id { get; set; } = -19999999210012;

        public List<byte[]> Arguments { get; set; }

        public string ErrorMsg { get; set; }

        public int ErrorId { get; set; }

        public bool IsError => ErrorId != 0;

        public bool IsHaveValue => Arguments != null && Arguments.Count > 0;

        public ResultValue this[int index]
        {            
            get
            {

                if (Arguments == null)
                    return null;

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


        public Result()
        {
            Arguments = new List<byte[]>();
        }

        public Result(params object[] args)
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
            if (IsError)            
                throw new ZYNETException(ErrorMsg, ErrorId);            

            return (T)Serialization.UnpackSingleObject(typeof(T), Arguments[index]);
        }

        public T As<T>()
        {
            if (IsError)            
                throw new ZYNETException(ErrorMsg, ErrorId);           

            if(Length<=0)
                throw new ZYNETException("null value",-102);

            return (T)Serialization.UnpackSingleObject(typeof(T), Arguments[0]);
        }

    }

    public class ResultAwatier : FiberThreadAwaiter<Result>
    {
        public ResultAwatier(Fiber GhostThread):base(GhostThread)
        {

        }

        public ResultAwatier():base()
        {

        }

    }
    
   
}
