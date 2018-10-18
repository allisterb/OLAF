using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;

using EasyHook;

using OLAF.ActivityDetectors.Windows;

namespace OLAF.Monitors.Windows
{
    public class EasyHookMonitor : AppHookMonitor
    {
        #region Constructors
        public EasyHookMonitor(int processId) : base(processId, typeof(FileActionsHook))
        {
        }
        #endregion

        #region Properties
        public IpcServerChannel Channel { get; protected set; }
        public string ChannelName { get; protected set; }
        public int ProcessId { get; protected set; }
        //public string 
        #endregion

        #region Overriden Methods
        public override bool Inject()
        {
            string channelName = null;
            Channel = RemoteHooking.IpcCreateServer<EasyHookIpcServerInterface>(ref channelName, WellKnownObjectMode.Singleton);
            ChannelName = channelName;
            string injectionLibrary = Path.Combine(AssemblyDirectory.FullName, HookAssemblyName);
            try
            {
                Info("Attempting to inject {0} assembly into process {1} ({2})...", injectionLibrary, 
                    ProcessID, Process.ProcessName);
                RemoteHooking.Inject(
                    ProcessID,          // ID of process to inject into
                    injectionLibrary,   // 32-bit library to inject (if target is 32-bit)
                    injectionLibrary,   // 64-bit library to inject (if target is 64-bit)
                    channelName,        // IPC chanel name
                    ProcessID,
                    typeof(EasyHookMonitor)
                );
                Info("Injected {0} assembly into process id {1} ({2}).", injectionLibrary, ProcessID, Process.ProcessName);
                return true;
            }
            catch (Exception e)
            {
                Error(e, "Exception thrown injecting {0} assembly into process id {1} ({2}).", injectionLibrary, ProcessID,
                    Process.ProcessName);
                return false;
            }
        }
        #endregion
    }
}
