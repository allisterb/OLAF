using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;

using EasyHook;

using Didac.Hooks.Windows.EasyHook;

namespace Didac.Monitor.Windows
{
    public class EasyHookMonitor : OSHookMonitor
    {
        public EasyHookMonitor(int processId, string hookAssemblyName) : base(processId, hookAssemblyName)
        {
        }

        public string ChannelName { get; protected set; }

        public override bool Init()
        {
            throw new NotImplementedException();
        }

        public override bool Ping()
        {
            throw new NotImplementedException();
        }

        public override bool Inject()
        {
            string channelName = null;
            // Create the IPC server using the IpcServiceInterface class as a singleton
            var channel = RemoteHooking.IpcCreateServer<IpcServerInterface>(ref channelName, WellKnownObjectMode.Singleton);
            string injectionLibrary = Path.Combine(AssemblyDirectory.FullName, HookAssemblyName);
            try
            {
                
                Info("Attempting to inject into process {0}", ProcessID);
                RemoteHooking.Inject(
                    ProcessID,          // ID of process to inject into
                    injectionLibrary,   // 32-bit library to inject (if target is 32-bit)
                    injectionLibrary,   // 64-bit library to inject (if target is 64-bit)
                    channelName         // the parameters to pass into injected library
                                        // ...
                );
                Info("Injected {0} into process {1} ({2})", injectionLibrary, ProcessID, Process.ProcessName);
                return true;
            }
            catch (Exception e)
            {
                Error(e, "Exception thrown injecting {0} into process {1} ({2})", injectionLibrary, ProcessID,
                    Process.ProcessName);
                return false;
            }
            
        }
    }
    
}
