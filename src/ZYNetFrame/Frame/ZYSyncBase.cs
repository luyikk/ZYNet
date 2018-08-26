using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.share;

namespace ZYNet.CloudSystem.Frame
{
    public abstract class ZYSyncBase:MessageExceptionParse
    {

        public Func<byte[],byte[]> DataExtra { get; set; }

        public void Action(int cmdTag, params object[] args )
        {
            CallPack buffer = new CallPack()
            {
                Id = Common.MakeID,       
                CmdTag=cmdTag,       
                Arguments = new List<byte[]>(args.Length)
            };

            foreach (var item in args)
            {
                Type type = item.GetType();

                buffer.Arguments.Add(Serialization.PackSingleObject(type, item));

            }


            using (MemoryStream stream = new MemoryStream())
            {

                BinaryWriter bufflist = new BinaryWriter(stream);

                if (DataExtra != null)
                {

                    bufflist.Write(CmdDef.CallCmd);
                 
                    bufflist.Write(buffer.Id);
                    bufflist.Write(buffer.CmdTag);
                    bufflist.Write(buffer.Arguments.Count);
                    foreach (var arg in buffer.Arguments)
                    {
                        bufflist.Write(arg.Length);
                        bufflist.Write(arg);
                    }

                    byte[] fdata = DataExtra(stream.ToArray());

                    stream.Position = 0;
                    stream.SetLength(0);
                    bufflist.Write(0);
                    bufflist.Write(fdata);
                }
                else
                {
                    bufflist.Write(0);
                    bufflist.Write(CmdDef.CallCmd);
                   
                    bufflist.Write(buffer.Id);
                    bufflist.Write(buffer.CmdTag);
                    bufflist.Write(buffer.Arguments.Count);
                    foreach (var arg in buffer.Arguments)
                    {
                        bufflist.Write(arg.Length);
                        bufflist.Write(arg);
                    }

                }

                int l = (int)(stream.Length);

                byte[] data = BufferFormat.GetSocketBytes(l);

                stream.Position = 0;

                bufflist.Write(data);


                byte[] pdata = stream.ToArray();


                SendData(pdata);

            }
        }        

        public Result Func(int cmdTag,params object[] args)
        {
            CallPack buffer = new CallPack()
            {
                Id = Common.MakeID,
                CmdTag = cmdTag,
                Arguments = new List<byte[]>(args.Length)
            };

            foreach (var item in args)
            {
                Type type = item.GetType();

                buffer.Arguments.Add(Serialization.PackSingleObject(type, item));

            }


            using (MemoryStream stream = new MemoryStream())
            {

                BinaryWriter bufflist = new BinaryWriter(stream);

                if (DataExtra != null)
                {

                    bufflist.Write(CmdDef.CallCmd);
                  
                    bufflist.Write(buffer.Id);
                    bufflist.Write(buffer.CmdTag);
                    bufflist.Write(buffer.Arguments.Count);
                    foreach (var arg in buffer.Arguments)
                    {
                        bufflist.Write(arg.Length);
                        bufflist.Write(arg);
                    }

                    byte[] fdata = DataExtra(stream.ToArray());

                    stream.Position = 0;
                    stream.SetLength(0);
                    bufflist.Write(0);
                    bufflist.Write(fdata);
                }
                else
                {
                    bufflist.Write(0);
                    bufflist.Write(CmdDef.CallCmd);
                   
                    bufflist.Write(buffer.Id);
                    bufflist.Write(buffer.CmdTag);
                    bufflist.Write(buffer.Arguments.Count);
                    foreach (var arg in buffer.Arguments)
                    {
                        bufflist.Write(arg.Length);
                        bufflist.Write(arg);
                    }

                }

                int l = (int)(stream.Length);

                byte[] data = BufferFormat.GetSocketBytes(l);

                stream.Position = 0;

                bufflist.Write(data);


                byte[] pdata = stream.ToArray();
              

                return SendDataAsWait(buffer.Id,pdata);

            }
        }

        protected abstract Result SendDataAsWait(long Id, byte[] Data);

        protected abstract void SendData(byte[] Data);      



    }
}
