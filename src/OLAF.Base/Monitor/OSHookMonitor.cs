using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class OSHookMonitor : Monitor
    {
        #region Constructors
        public OSHookMonitor(int processId, Type hookType) : base()
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
            HookType = hookType ?? throw new ArgumentNullException(nameof(hookType));
            HookAssemblyName = HookType.Assembly.FullName.Split(',').First() + ".dll";
            Initialized = true;
        }
        #endregion

        #region Properties
        public bool Initialized { get; } = false;
        public Process Process { get; }
        public int ProcessID { get; }
        public Type HookType { get; }
        public string HookAssemblyName { get; }
        #endregion

        #region Methods
        public abstract bool Inject();
        #endregion

    }
}
