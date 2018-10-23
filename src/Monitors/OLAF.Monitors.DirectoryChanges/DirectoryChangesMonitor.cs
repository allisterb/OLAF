using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using OLAF.ActivityDetectors;
namespace OLAF.Monitors
{
    public class DirectoryChangesMonitor : FileSystemMonitor<FileSystemActivity, FileSystemChangeMessage>
    {
        #region Constructors
        public DirectoryChangesMonitor(Dictionary<string, string> paths, Profile profile = null) : base(paths, profile) {}

        public DirectoryChangesMonitor(string[] dirs, string[] exts, Profile profile = null) : base(dirs, exts, profile) { }
        #endregion

        #region Overridden members
        public override ApiResult Init()
        {
            if (Status != ApiStatus.Initializing) return ApiResult.Failure;
            try
            {
                Watchers = new FileSystemActivity[Paths.Keys.Count];
                for (int i = 0; i < Paths.Count; i++)
                {
                    KeyValuePair<DirectoryInfo, string> path = Paths.ElementAt(i);
                    Watchers[i] = new FileSystemActivity(path.Key.FullName, path.Value, typeof(DirectoryChangesMonitor));
                    Info("Monitoring path {0}.", Path.Combine(path.Key.FullName, path.Value));
                }
                Status = ApiStatus.Initialized;
                return ApiResult.Success;
            }
            catch (Exception e)
            {
                Error(e, "Error occurred initializing a watcher.");
                Status = ApiStatus.Error;
                return ApiResult.Failure;
            }
        }

        public override ApiResult Start()
        {
            QueueMonitorThread = new Thread(() => MonitorQueue(Global.CancellationTokenSource.Token));
            Threads = new List<Thread>() { QueueMonitorThread };
            QueueMonitorThread.Start();
            int enabled = 0;
            foreach(FileSystemActivity fsa in Watchers)
            {
                if (fsa.EnableEvents() == ApiResult.Success)
                {
                    enabled++;
                }
                else
                {
                    Error("Could not enable filesystem events for {0}.", fsa.Path);
                }
            }
            if (enabled > 0)
            {
                Status = ApiStatus.Ok;
                return ApiResult.Success;
            }
            else
            {
                Status = ApiStatus.Error;
                return ApiResult.Failure;
            }
        }

        public override ApiResult Shutdown()
        {
            shutdownRequested = true;
            if (!cancellationToken.IsCancellationRequested)
            {
                Global.CancellationTokenSource.Cancel();
            }
            int waitCount = 0;
            while(Threads.Any(t =>  t.IsAlive) && waitCount < 30)
            {
                Thread.Sleep(100);
            }
            if (Threads.All(t => !t.IsAlive))
            {
                shutdownCompleted = true;
                Info("{0} shutdown complete.", typeof(DirectoryChangesMonitor).Name);
                return ApiResult.Success;
            }
            else
            {
                Info("{0} threads in {1} did not shutdown.", Threads.Count(t => !t.IsAlive), typeof(DirectoryChangesMonitor).Name);
                return ApiResult.Failure;
            }
        }

        protected override ApiResult ProcessQueueMessage(FileSystemChangeMessage message)
        {
            Info(message.Path);
            return ApiResult.Success;
        }
        #endregion

        #region Properties
        protected FileSystemActivity[] Watchers { get; set; }
        #endregion
    }
}
