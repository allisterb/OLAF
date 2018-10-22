using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class AppMonitor : Monitor
    {
        #region Constructors
        public AppMonitor(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes == null || processes.Length == 0)
            {
                Error("No processes to monitor.");
                Status = ApiStatus.ProcessNotFound;
                return;
            }
            else
            {
                Processes = processes;
                ProcessName = processName;
                Status = ApiStatus.Initializing;
            }
        }
        #endregion

        #region Properties
        public string ProcessName { get; protected set; }

        public Process[] Processes { get; protected set; }
        #endregion
    }
}
