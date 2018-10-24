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
        public DirectoryChangesMonitor(Dictionary<string, string> paths, Profile profile) : base(paths, profile) {}

        public DirectoryChangesMonitor(string[] dirs, string[] exts, Profile profile) : base(dirs, exts, profile) {}
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
                }
                Info("Monitoring {0} paths for files with extension(s) {1}.", Paths.Count,
                    Paths.Values.Distinct());
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
            string artifactName = string.Format("{0}_{1}", message.Id, Path.GetFileName(message.Path));
            string artifactPath = Profile.GetArtifactsDirectoryPathTo(artifactName);
            if (TryCopyLockedFileToPath(message.Path, artifactPath))
            {
                Debug("Copied artifact {0} to {1}.", message.Path, artifactPath);
                return ApiResult.Success;
            }
            else
            {
                Error("Could not copy artifact {0} to {1}.", message.Path, artifactPath);
                return ApiResult.Success;
            }
            
        }

        protected bool TryCopyLockedFileToPath(string oldPath, string newPath, int maxTries = 60)
        {
            int tries = 0;
            while (tries < maxTries)
            {
                try
                {
                    File.Copy(oldPath, newPath, false);
                    return true;
                }
                catch (IOException)
                {
                    Thread.Sleep(50);
                    tries++;
                }
            }
            return false;
        }
        #endregion
    }
}
