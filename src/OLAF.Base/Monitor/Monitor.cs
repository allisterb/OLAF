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
        public Monitor(string name)
        {
            var processes = Process.GetProcessesByName(name);
            if (processes == null || processes.Length == 0)
            {
                Error("No processes to monitor.");
                Status = ApiStatus.ProcessNotFound;
                return;
            }
            else
            {
                Processes = processes;
                ProcessName = name;
                Status = ApiStatus.Initializing;
            }
        }
        #endregion

        #region Properties
        public string ProcessName { get; protected set; }

        public Process[] Processes { get; protected set; }

        public Thread QueueMonitorThread { get; protected set; }
        #endregion

        #region Abstract methods
        public abstract ApiResult Init();
        public abstract ApiResult Start();
        public abstract ApiResult Stop();
        public abstract ApiResult Shutdown();

        protected abstract void MonitorQueue(CancellationToken token);
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
