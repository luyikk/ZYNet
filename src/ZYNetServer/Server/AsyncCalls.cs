using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.Loggine;
using ZYSocket.share;


namespace ZYNet.CloudSystem.Server
{
    public class AsyncCalls: IFodyCall
    {
        protected static readonly ILog Log = LogFactory.ForContext<AsyncCalls>();

        public Result Result { get; private set; }

        private Dictionary<Type, Type> FodyDir { get; set; }

        public ASyncToken AsyncUser { get; private set; }

        public bool IsOver { get; private set; }
        public bool IsError { get; private set; }
        public Exception Error { get; private set; }

        internal event Action<Result> Complete;

        internal event Action<byte[]> CallSend;

        public Fiber _fiber { get; private set; }

        public bool IsHaveReturn { get; private set; }

        public MethodInfo Method { get; private set; }

        public object[] Args { get; private set; }

        public long Id { get; private set; }

        public int Cmd { get; private set; }

        public object UserToken
        {
            get { return AsyncUser?.UserToken; }
            set { if (AsyncUser != null) AsyncUser.UserToken = value; }
        }

        public bool IsValidate
        {
            get { return AsyncUser.IsValidate; }
            set { if (AsyncUser != null) AsyncUser.IsValidate = value; }
        }

        public T Token<T>()
        {
            return AsyncUser.Token<T>();
        }


        public CloudServer CurrentServer => AsyncUser?.CurrentServer;


        public AsyncCalls(ASyncToken token,Fiber fiber)
        {
            this.AsyncUser = token;
            this._fiber = fiber;
            FodyDir = new Dictionary<Type, Type>();
        }

        ~AsyncCalls()
        {
            _fiber?.Dispose();
        }


        public AsyncCalls(long id,int cmd,ASyncToken token,MethodInfo method,object[] args,bool ishavereturn)
        {
            IsHaveReturn = ishavereturn;
            Method = method;
            Args = args;
            AsyncUser = token;
            Id = id;
            FodyDir = new Dictionary<Type, Type>();
        }

        public void Run()
        {

            Func<Task> wrappedGhostThreadFunction = async () =>
            {
                try
                {
                    if (IsHaveReturn)
                    {
                        Result = await (Task<Result>)Method.Invoke(null, Args);

                        Complete?.Invoke(Result);
                    }
                    else
                    {
                        await (Task)Method.Invoke(null, Args);
                    }

                }
                catch (Exception er)
                {
                    IsError = true;
                    Error = er;

                    if (IsHaveReturn)
                    {
                        var nullx = new Result()
                        {
                            Id = this.Id,
                            ErrorMsg = er.ToString(),
                            ErrorId = er.HResult
                        };
                        Complete?.Invoke(nullx);
                    }

                    Log.Error($"Cmd:{Cmd} Error:\r\n{Error}",er);

                }
                finally
                {
                    IsOver = true;
                    AsyncUser.RemoveAsyncCall(Id);
                }
            };


            _fiber = new Fiber();
            _fiber.SetAction(wrappedGhostThreadFunction);
            _fiber.Start();

        }

        /// <summary>
        /// Need Nuget Install-Package Fody
        /// And Add xml file 'FodyWeavers.xml' to project
        /// context:
        /// \<?xml version="1.0" encoding="utf-8" ?\>
        /// \<Weavers\>
        /// \<Virtuosity\/\> 
        /// \</Weavers\>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>()
        {
            var interfaceType = typeof(T);
            if (!FodyDir.ContainsKey(interfaceType))
            {
                var assembly = interfaceType.Assembly;
                var implementationType = assembly.GetType(interfaceType.FullName + "_Builder_Implementation");
                if (implementationType == null)
                    throw new Exception("not find with {interfaceType.FullName} the Implementation");
                FodyDir.Add(interfaceType, implementationType);
                return (T)Activator.CreateInstance(implementationType, new Func<int, Type, object[], object>(Call));

            }
            else
            {
                return (T)Activator.CreateInstance(FodyDir[interfaceType], new Func<int, Type, object[], object>(Call));

            }
        }


        public virtual object Call(int cmd, Type returnType, object[] args)
        {
      
            if (returnType != typeof(void))
            {

                if (!Common.IsTypeOfBaseTypeIs(returnType, typeof(FiberThreadAwaiterBase)))
                {
                    throw new Exception(string.Format("Async Call Not Use Sync Mehhod"));
                }
                else
                {
                    return Func(cmd, args);
                }
            }
            else
            {
                Action(cmd, args);

                return null;
            }


        }



#if !Xamarin

        public T GetForEmit<T>()
        {
            var tmp = DispatchProxy.Create<T, SyncProxy>();
            var proxy = tmp as SyncProxy;
            proxy.Call = Call;
            return tmp;
        }


        protected virtual object Call(MethodInfo method, object[] args)
        {

            var attr = method.GetCustomAttribute(typeof(TAG), true);

            if (attr == null)
            {
                throw new FormatException(method.Name + " Is Not MethodRun Attribute");
            }

            if (attr is TAG run)
            {
                int cmd = run.CmdTag;

                if (method.ReturnType != typeof(void))
                {


                    if (!Common.IsTypeOfBaseTypeIs(method.ReturnType, typeof(FiberThreadAwaiterBase)))
                    {
                        throw new Exception(string.Format("Async Call Not Use Sync Mehhod"));
                    }
                    else
                    {
                        return Func(cmd, args);
                    }
                }
                else
                {
                    Action(cmd, args);

                    return null;
                }

            }
            else
                return null;
        }

#endif

        /// <summary>
        /// CALL VOID
        /// </summary>
        /// <param name="cmdTag"></param>
        /// <param name="args"></param>
        public void Action(int cmdTag, params object[] args)
        {
            AsyncUser.Action(cmdTag, args);
        }

        /// <summary>
        /// CALL RETURN
        /// </summary>
        /// <param name="cmdTag"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ResultAwatier Func(int cmdTag, params object[] args)
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

                if (AsyncUser.DataExtra != null)
                {

                    bufflist.Write(CmdDef.CallCmd);
                    byte[] classdata = BufferFormat.SerializeObject(buffer);
                    bufflist.Write(classdata.Length);
                    bufflist.Write(classdata);

                    byte[] fdata = AsyncUser.DataExtra(stream.ToArray());

                    stream.Position = 0;
                    stream.SetLength(0);
                    bufflist.Write(0);
                    bufflist.Write(fdata);
                }
                else
                {
                    bufflist.Write(0);
                    bufflist.Write(CmdDef.CallCmd);
                    byte[] classdata = BufferFormat.SerializeObject(buffer);
                    bufflist.Write(classdata.Length);
                    bufflist.Write(classdata);

                }

                int l = (int)(stream.Length);

                byte[] data = BufferFormat.GetSocketBytes(l);

                stream.Position = 0;

                bufflist.Write(data);


                byte[] pdata = stream.ToArray();

                AsyncUser.AddAsyncCallBack(this, buffer.Id);

                CallSend?.Invoke(pdata);
            }

            return _fiber.Read();
        }


        public void SetRes(Result result)
        {
            _fiber.Set(result);
        }

        public Result Res(params object[] args)
        {
            Result tmp = new Result(args)
            {
                Id = this.Id
            };
            return tmp;
        }

        public void Disconnect()
        {           
            if (AsyncUser != null)
                AsyncUser.Disconnect();
        }
    }
}
