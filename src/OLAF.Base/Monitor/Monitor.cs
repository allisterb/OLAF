using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class Monitor : OLAFApi<Monitor>
    {
        #region Abstract methods
        public abstract ApiResult Init();
        protected abstract void MonitorQueue(CancellationToken token);
        #endregion

        #region Properties
        public Thread QueueMonitorThread { get; protected set; }
        
        public Profile Profile { get; protected set; }

        public bool ShutdownRequested => shutdownRequested;

        public bool ShutdownCompleted => shutdownCompleted;

        protected List<Thread> Threads { get; set; }

        protected List<ActivityDetector> Detectors { get; set; }

        #endregion

        #region Methods
        public virtual ApiResult Start()
        {
            QueueMonitorThread = new Thread(() => MonitorQueue(Global.CancellationTokenSource.Token));
            Threads = new List<Thread>() { QueueMonitorThread };
            QueueMonitorThread.Start();
            int enabled = 0;
            foreach (ActivityDetector d in Detectors)
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
                Info("{0} shutdown complete.", this.GetType().Name);
                return ApiResult.Success;
            }
            else
            {
                Info("{0} threads in {1} did not shutdown.", Threads.Count(t => t.IsAlive),
                    this.GetType().Name);
                return ApiResult.Failure;
            }
        }


        protected static Process GetProcessById(int id)
        {
            try
            {
                return Process.GetProcessById(id);
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region Fields
        protected bool shutdownRequested = false;
        protected bool shutdownCompleted = false;
        #endregion
    }
}
