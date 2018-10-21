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
        #region Constructors
        public Monitor(int pid)
        {
            try
            {
                Process = Process.GetProcessById(pid);
                ProcessID = pid;
                Status = ApiStatus.Initializing;
            }
            catch (Exception e)
            {
                Error(e, "Exception occurred finding process with ID {0}.", pid);
                Status = ApiStatus.FileNotFound;
                return;
            }
        }
        #endregion

        #region Properties
        public int ProcessID { get; protected set; }

        public Process Process { get; protected set; }

        public Thread QueueMonitorThread { get; protected set; }
        #endregion

        #region Abstract methods
        public abstract ApiResult Init();
        public abstract ApiResult Start();
        public abstract ApiResult Stop();
        public abstract ApiResult Shutdown();

        protected abstract void MonitorQueue(CancellationToken token);
        #endregion
    }
}
