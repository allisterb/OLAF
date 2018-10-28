using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class Monitor<TDetector, TDetectorMessage, TMonitorMessage> : 
        OLAFApi<Monitor<TDetector, TDetectorMessage, TMonitorMessage>, TMonitorMessage>, 
        IMonitor, IQueueProducer
        where TDetector : ActivityDetector<TDetectorMessage>
        where TDetectorMessage : Message
        where TMonitorMessage : Message
    {
        #region Abstract methods
        public abstract ApiResult Init();
        protected abstract ApiResult ProcessDetectorQueueMessage(TDetectorMessage message);
        #endregion

        #region Properties
        public Type QueueMessageType { get; } = typeof(TMonitorMessage);

        public Thread QueueObserverThread { get; protected set; }
        
        public Profile Profile { get; protected set; }

        public bool ShutdownRequested => shutdownRequested;

        public bool ShutdownCompleted => shutdownCompleted;

        public List<Thread> Threads { get; protected set; }

        protected List<TDetector> Detectors { get; set; }

        #endregion

        #region Methods
        public virtual ApiResult Start()
        {
            ThrowIfNotInitialized();
            QueueObserverThread = new Thread(() => ObserveDetectorQueue(Global.CancellationTokenSource.Token));
            Threads = new List<Thread>() { QueueObserverThread };
            QueueObserverThread.Start();
            int enabled = 0;
            foreach (TDetector d in Detectors)
            {
                if (d.Enable() == ApiResult.Success)
                {
                    enabled++;
                }
                else
                {
                    Error("Could not enable detector.");
                }
            }
            if (enabled > 0)
            {
                Status = ApiStatus.Ok;
                return ApiResult.Success;
            }
            else
            {
                Status = ApiStatus.Error;
                return ApiResult.Failure;
            }
        }

        public virtual ApiResult Shutdown()
        {
            ThrowIfNotOk();
            shutdownRequested = true;
            if (!cancellationToken.IsCancellationRequested)
            {
                Global.CancellationTokenSource.Cancel();
            }
            int waitCount = 0;
            while (Threads.Any(t => t.IsAlive) && waitCount < 30)
            {
                Thread.Sleep(100);
                waitCount++;
            }
            if (Threads.All(t => !t.IsAlive))
            {
                shutdownCompleted = true;
                Info("All threads stopped. {0} monitor shutdown completed successfully.", Name);
                return ApiResult.Success;
            }
            else
            {
                Info("{0} threads in {1} did not stop. Aborting {0} threads", Threads.Count(t => t.IsAlive),
                    this.GetType().Name);
                foreach(Thread thread in Threads.Where(t => t.IsAlive))
                {
                    thread.Abort();
                }
                if (Threads.All(t => !t.IsAlive))
                {
                    shutdownCompleted = true;
                    Info("All threads stopped. {0} monitor shutdown completed successfully.", Name);
                    return ApiResult.Success;
                }
                else
                {
                    return ApiResult.Failure;
                }
            }
        }

        protected virtual void ObserveDetectorQueue(CancellationToken token)
        {
            try
            {
                while (!shutdownRequested && !token.IsCancellationRequested)
                {
                    TDetectorMessage message =
                        (TDetectorMessage)Global.MessageQueue.Dequeue<TDetector>(cancellationToken);
                    ProcessDetectorQueueMessage(message);
                }
                Info("Stopping {0} detector queue observer in monitor {1}.", typeof(TDetector).Name, Name);
                Status = ApiStatus.Ok;
                return;
            }
            catch (OperationCanceledException)
            {
                Info("Stopping {0} detector queue observer in monitor {1}.", typeof(TDetector).Name, Name);
                Status = ApiStatus.Ok;
                return;
            }
            catch (Exception ex)
            {
                Error(ex, "Error occurred during {0} detector queue observing in {1}. Resuming.", typeof(TDetector).Name, Name);
                ObserveDetectorQueue(token);
            }
        }
        #endregion

        #region Fields
        protected bool shutdownRequested = false;
        protected bool shutdownCompleted = false;
        #endregion
    }
}
