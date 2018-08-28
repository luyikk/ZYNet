using Autofac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;
using ZYSocket.share;

namespace ZYNet.CloudSystem.Client
{
    public class AsyncRun : AsyncRunBase
    {
        public CloudClient CCloudClient { get; private set; }       

        internal Action<byte[]> CallSend;

        public IContainer Container => CCloudClient.Container;

        public AsyncRun(CloudClient client)
        {            
            CCloudClient = client;          
        }


        /// <summary>
        /// CALL RETURN
        /// </summary>
        /// <param name="cmdTag"></param>
        /// <param name="args"></param>
        /// <returns></returns>

        public override ResultAwatier Func(int cmdTag, params object[] args)
        {
            CallPack buffer = new CallPack()
            {
                Id = Common.MakeID,
                CmdTag = cmdTag,
                Arguments = new List<byte[]>(args.Length)
            };

            this.Id = buffer.Id;

            foreach (var item in args)
            {
                Type type = item.GetType();

                buffer.Arguments.Add(Serialization.PackSingleObject(type, item));

            }


            using (MemoryStream stream = new MemoryStream())
            {

                BinaryWriter bufflist = new BinaryWriter(stream);

                if (CCloudClient.EncodingHandler != null)
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

                    byte[] fdata = CCloudClient.EncodingHandler(stream.ToArray());

                    stream.Position = 0;
                    stream.SetLength(0);
                    bufflist.Write(0);
                    bufflist.Write(fdata);
                }
                else
                {
                    bufflist.Write(0);
                    bufflist.Write(CmdDef.CallCmd);
                    //byte[] classdata = BufferFormat.SerializeObject(buffer);
                    //bufflist.Write(classdata.Length);
                    //bufflist.Write(classdata);
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


                CCloudClient.AddAsyncRunBack(this, buffer.Id);

                CallSend?.Invoke(pdata);
            }

             
            if (awaiter == null)
                awaiter = new ResultAwatier();

            return base.awaiter;
        }

        /// <summary>
        /// CALL VOID
        /// </summary>
        /// <param name="cmdTag"></param>
        /// <param name="args"></param>
        public override void Action(int cmdTag, params object[] args)
        {
            CCloudClient.Sync.Action(cmdTag, args);
        }
    }
}
