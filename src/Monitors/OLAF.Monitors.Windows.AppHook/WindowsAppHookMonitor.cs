using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using EasyHook;

using OLAF.ActivityDetectors.Windows;

namespace OLAF.Monitors.Windows
{
    public class WindowsAppHookMonitor<TDetector, TMessage> : Monitor 
        where TDetector : ActivityDetector
        where TMessage : Message
    {
        #region Constructors
        public WindowsAppHookMonitor(int processId) : base(processId)
        {
            if (Status != ApiStatus.Initializing)
            {
                return;
            }
            HookAssemblyName = typeof(TDetector).Assembly.FullName.Split(',').First() + ".dll";
            if (File.Exists(Path.Combine(AssemblyDirectory.FullName, HookAssemblyName)))
            {
                Status = ApiStatus.Ok;
            }
            else
            {
                Status = ApiStatus.FileNotFound;
                Error("Could not locate assembly {0}.", HookAssemblyName);
            }
        }
        #endregion

        #region Overriden members
        public override ApiResult Init()
        {
            if (Inject())
            {
                Status = ApiStatus.Initialized;
                return ApiResult.Success;
            }
            else
            {
                Status = ApiStatus.Error;
                return ApiResult.Failure;
            }
            
        }

        public override ApiResult Start()
        {
            QueueMonitorThread = new Thread(() => MonitorQueue(cancellationToken));
            QueueMonitorThread.Start();
            return ApiResult.Success;
        }

        public override ApiResult Stop()
        {
            if(!cancellationToken.IsCancellationRequested)
            {
                Global.CancellationTokenSource.Cancel();
            }
            return ApiResult.Success;
        }

        public override ApiResult Shutdown()
        {
            return ApiResult.Success;
        }

        protected override void MonitorQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Message msg = Global.MessageQueue.Dequeue<TDetector>(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    Info("Stopping queue monitor");
                    Status = ApiStatus.Ok;
                    return;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
            
        }
        #endregion

        #region Properties
        public Type HookType { get; }
        public string HookAssemblyName { get; }
        public IpcServerChannel Channel { get; protected set; }
        public string ChannelName { get; protected set; }
        #endregion

        #region Methods
        protected virtual bool Inject()
        {
            string channelName = null;
            Channel = RemoteHooking.IpcCreateServer<EasyHookIpcInterface>(ref channelName, WellKnownObjectMode.Singleton);
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
                    ChannelName,        // IPC chanel name
                    ProcessID,
                    typeof(WindowsAppHookMonitor<TDetector, TMessage>)
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
