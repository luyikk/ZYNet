#if!Net2
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZYNet.CloudSystem.Frame
{
    public class Fiber
    {
        ConcurrentQueue<ResultAwatier> receivers = new ConcurrentQueue<ResultAwatier>();
        ConcurrentQueue<ResultAwatier> senders = new ConcurrentQueue<ResultAwatier>();

        private Func<Task> Action { get; set; }

        private CancellationTokenSource cancellationTokenSource;
        public CancellationToken CancellationToken => cancellationTokenSource.Token;
        
        private SynchronizationContext previousSyncContext { get; set; }

        private FiberSynchronizationContext _SynchronizationContext { get; set; }

        public bool IsOver { get; private set; }


        public bool IsError { get; private set; }

        public Exception  Error { get; private set; }

        public Fiber()
        {
            _SynchronizationContext = new FiberSynchronizationContext(this);
            cancellationTokenSource = new CancellationTokenSource();
        }


        public static Fiber Current => (SynchronizationContext.Current as FiberSynchronizationContext)?.fiber;

        public void SetAction(Func<Task> action)
        {
            Action = action;
        }

        public void Start()
        {
            IsOver = false;

            Action wrappedGhostThreadFunction = async () =>
            {
                try
                {
                    await Action();


                }
                catch (Exception er)
                {
                    
                    IsError = true;
                    Error = er;
                }
                finally
                {
                    IsOver = true;
                }
            };

            var previousSyncContext = SynchronizationContext.Current;

            SynchronizationContext.SetSynchronizationContext(_SynchronizationContext);          

            wrappedGhostThreadFunction();

            SynchronizationContext.SetSynchronizationContext(previousSyncContext);
        }

        public void Dispose()
        {
            cancellationTokenSource.Dispose();
        }

        public void Close()
        {
            cancellationTokenSource.Cancel();
        }


        public ResultAwatier Set(ReturnResult data)
        {
            if (receivers.Count == 0)
            {
                
                var GhostThread = Fiber.Current;

                if (GhostThread == null)
                    GhostThread = this;

                var waitingGhostThread =new ResultAwatier(GhostThread);
                waitingGhostThread.Result = data;

                senders.Enqueue(waitingGhostThread);
                return waitingGhostThread;

            }



            ResultAwatier tmp;

            if (receivers.TryDequeue(out tmp))
            {
                var receiver = tmp as ResultAwatier;

                receiver.Result = data;
                receiver.IsCompleted = true;

                var previousSyncContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(_SynchronizationContext);
                if(receiver.Continuation!=null)
                    receiver.Continuation();
                SynchronizationContext.SetSynchronizationContext(previousSyncContext);
                return receiver;
            }
            else
                return null;

        }


        public ResultAwatier Get()
        {

            if (senders.Count == 0)
            {
                var GhostThread = Fiber.Current;

                if (GhostThread == null)
                    GhostThread = this;

                var waitingGhostThread = new ResultAwatier(GhostThread);
                receivers.Enqueue(waitingGhostThread);
                return waitingGhostThread;

            }

            ResultAwatier sender;

            if (senders.TryDequeue(out sender))
            {
                sender.IsCompleted = true;             
                return sender;
            }
            else
                return null;
        }

     

        public ResultAwatier Read()
        {


            if (senders.Count == 0)
            {
                var GhostThread = Fiber.Current;

                if (GhostThread == null)
                    GhostThread = this;

                var waitingGhostThread = new ResultAwatier(GhostThread);

                receivers.Enqueue(waitingGhostThread);
                return waitingGhostThread;
            }

            ResultAwatier sender;

            if (senders.TryPeek(out sender))
            {
                sender.IsCompleted = true;             
                return sender;
            }
            else
                return null;
        }

        public ResultAwatier Back()
        {


            if (senders.Count == 0)
            {
                var GhostThread = Fiber.Current;

                if (GhostThread == null)
                    GhostThread = this;

                var waitingGhostThread = new ResultAwatier(GhostThread);

                receivers.Enqueue(waitingGhostThread);
            }

            ResultAwatier sender;

            if (senders.TryDequeue(out sender))
            {
                sender.IsCompleted = true;

                var previousSyncContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(_SynchronizationContext);
                if(sender.Continuation!=null)
                    sender.Continuation();
                SynchronizationContext.SetSynchronizationContext(previousSyncContext);
               
               
                return sender;
            }
            else
                return null;
        }

        public ResultAwatier Send(ReturnResult data)
        {
           
            var GhostThread = Fiber.Current;

            if (GhostThread == null)
                GhostThread = this;


            var waitingGhostThread = new ResultAwatier(GhostThread);
            waitingGhostThread.Result = data;
            senders.Enqueue(waitingGhostThread);
            return waitingGhostThread;
        }

    }
}
#endif