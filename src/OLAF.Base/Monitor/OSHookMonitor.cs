using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class OSHookMonitor : OLAFApi<OSHookMonitor>
    {
        #region Constructors
        public OSHookMonitor(int processId, string hookAssemblyName) : base()
        {

            try
            {
                Process = Process.GetProcessById(processId);
                ProcessID = processId;
            }
            catch(Exception e)
            {
                Error(e, "Exception occurred finding process with ID {0}.", processId);
                return;
            }
            if (File.Exists(hookAssemblyName))
            {
                HookAssemblyName = hookAssemblyName;
            }
            else
            {
                Error("The file {0} could not be found.", hookAssemblyName);
                return;
            }
            HookAssemblyName = hookAssemblyName;
            Initialized = true;
        }
        #endregion

        #region Properties
        public bool Initialized { get; } = false;
        public Process Process { get; }
        public int ProcessID { get; }
        public string HookAssemblyName { get; }
        #endregion

        #region Methods
        public abstract bool Inject();
        #endregion

    }
}
