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
                Detectors = new List<ActivityDetector>(Paths.Count);
                for (int i = 0; i < Paths.Count; i++)
                {
                    KeyValuePair<DirectoryInfo, string> path = Paths.ElementAt(i);
                    Detectors.Add(new FileSystemActivity(path.Key.FullName, path.Value, 
                        typeof(DirectoryChangesMonitor)));
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

        protected override ApiResult ProcessQueueMessage(FileSystemChangeMessage message)
        {
            Info(message.Path);
            return ApiResult.Success;
        }
        #endregion
    }
}
