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
        public abstract ApiResult Start();
        public abstract ApiResult Stop();
        public abstract ApiResult Shutdown();

        protected abstract void MonitorQueue(CancellationToken token);
        #endregion

        #region Properties
        public Thread QueueMonitorThread { get; protected set; }
        #endregion

        #region Methods
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
    }
}
