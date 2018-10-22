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
        public DirectoryChangesMonitor(Dictionary<string, string> paths) : base(paths) {}
        #endregion

        #region Overridden members
        public override ApiResult Init()
        {
            if (Status != ApiStatus.Ok) return ApiResult.Failure;
            try
            {
                Watchers = new FileSystemActivity[Paths.Keys.Count];
                for (int i = 0; i < Paths.Count; i++)
                {
                    KeyValuePair<DirectoryInfo, string> path = Paths.ElementAt(i);
                    Watchers[i] = new FileSystemActivity(path.Key.FullName, path.Value, typeof(DirectoryChangesMonitor));
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
            QueueMonitorThread.Start();
            foreach(FileSystemActivity fsa in Watchers)
            {
                fsa.EnableEvents();
            }
            return ApiResult.Success;
        }

        public override ApiResult Shutdown()
        {
            throw new NotImplementedException();
        }


        public override ApiResult Stop()
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                Global.CancellationTokenSource.Cancel();
            }
            return ApiResult.Success;
        }

        #endregion

        #region Properties
        protected FileSystemActivity[] Watchers { get; set; }
        #endregion
    }
}
