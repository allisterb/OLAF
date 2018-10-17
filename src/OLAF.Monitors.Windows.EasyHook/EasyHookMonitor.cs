using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;

using EasyHook;

using OLAF.Hooks.Windows;

namespace OLAF.Monitors.Windows
{
    public class EasyHookMonitor : OSHookMonitor
    {
        #region Constructors
        public EasyHookMonitor(int processId, string hookAssemblyName) : base(processId, hookAssemblyName)
        {
        }
        #endregion

        #region Properties
        public IpcServerChannel Channel { get; protected set; }
        public string ChannelName { get; protected set; }
        #endregion

        #region Overriden Methods
        public override bool Inject()
        {
            string channelName = null;
            // Create the IPC server using the IpcServiceInterface class as a singleton
            Channel = RemoteHooking.IpcCreateServer<EasyHookIpcServerInterface>(ref channelName, WellKnownObjectMode.Singleton);
            ChannelName = channelName;
            string injectionLibrary = Path.Combine(AssemblyDirectory.FullName, HookAssemblyName);
            try
            {
                
                Info("Attempting to inject {0} into process {1} ({2})...", injectionLibrary, 
                    ProcessID, Process.ProcessName);
                RemoteHooking.Inject(
                    ProcessID,          // ID of process to inject into
                    injectionLibrary,   // 32-bit library to inject (if target is 32-bit)
                    injectionLibrary,   // 64-bit library to inject (if target is 64-bit)
                    channelName         // the parameters to pass into injected library
                                        // ...
                );
                Info("Injected {0} into process {1} ({2}).", injectionLibrary, ProcessID, Process.ProcessName);
                return true;
            }
            catch (Exception e)
            {
                Error(e, "Exception thrown injecting {0} into process {1} ({2}).", injectionLibrary, ProcessID,
                    Process.ProcessName);
                return false;
            }
            
        }
        #endregion
    }

}
