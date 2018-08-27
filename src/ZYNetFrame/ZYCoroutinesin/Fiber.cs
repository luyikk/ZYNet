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
    public class Fiber:IDisposable
    {
        ConcurrentQueue<ResultAwatier> receivers = new ConcurrentQueue<ResultAwatier>();
        ConcurrentQueue<ResultAwatier> senders = new ConcurrentQueue<ResultAwatier>();

        private Func<Task> Action { get; set; }

        private CancellationTokenSource cancellationTokenSource;
        public CancellationToken CancellationToken => cancellationTokenSource.Token;
        
        private SynchronizationContext PreviousSyncContext { get; set; }

        private FiberSynchronizationContext _SynchronizationContext { get; set; }

        public bool IsOver { get; private set; }


        public bool IsError { get; private set; }

        public Exception  Error { get; private set; }

        public Fiber()
        {
            _SynchronizationContext = new FiberSynchronizationContext(this);
            cancellationTokenSource = new CancellationTokenSource();
        }


        public static Fiber Current => (SynchronizationContext.Current as FiberSynchronizationContext)?.Fiber;

        public void SetAction(Func<Task> action)
        {
            Action = action;
        }

        public void Start()
        {
            IsOver = false;

            async void wrappedGhostThreadFunction()
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
            }

            var previousSyncContext = SynchronizationContext.Current;

            SynchronizationContext.SetSynchronizationContext(_SynchronizationContext);          

            wrappedGhostThreadFunction();

            SynchronizationContext.SetSynchronizationContext(previousSyncContext);
        }

        public void Close()
        {
            cancellationTokenSource.Cancel();           
        }


        public ResultAwatier Set(Result data)
        {
            if (receivers.Count == 0)
            {
                
                var GhostThread = Fiber.Current;

                if (GhostThread == null)
                    GhostThread = this;

                var waitingGhostThread = new ResultAwatier(GhostThread)
                {
                    Result = data
                };

                senders.Enqueue(waitingGhostThread);
                return waitingGhostThread;

            }



           

            if (receivers.TryDequeue(out ResultAwatier tmp))
            {
                var receiver = tmp as ResultAwatier;
                receiver.Result = data;
                receiver.IsCompleted = true;

                var previousSyncContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(_SynchronizationContext);
                receiver.Continuation?.Invoke();
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
                     

            if (senders.TryDequeue(out ResultAwatier sender))
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
            
            if (senders.TryPeek(out ResultAwatier sender))
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

           
            if (senders.TryDequeue(out ResultAwatier sender))
            {
                sender.IsCompleted = true;

                var previousSyncContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(_SynchronizationContext);
                sender.Continuation?.Invoke();
                SynchronizationContext.SetSynchronizationContext(previousSyncContext);
               
               
                return sender;
            }
            else
                return null;
        }

        public ResultAwatier Send(Result data)
        {
           
            var GhostThread = Fiber.Current;

            if (GhostThread == null)
                GhostThread = this;


            var waitingGhostThread = new ResultAwatier(GhostThread)
            {
                Result = data
            };
            senders.Enqueue(waitingGhostThread);
            return waitingGhostThread;
        }

        public void Dispose()
        {
            cancellationTokenSource.Dispose();
        }
    }
}
#endif