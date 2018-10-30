using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using OLAF.ActivityDetectors;

namespace OLAF.Monitors
{
    public class DirectoryChangesMonitor : FileSystemMonitor<FileSystemActivity, FileSystemChangeMessage, FileArtifact>
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
                
                for (int i = 0; i < Paths.Count; i++)
                {
                    KeyValuePair<DirectoryInfo, string> path = Paths.ElementAt(i);
                    Detectors.Add(new FileSystemActivity(path.Key.FullName, path.Value, 
                        typeof(DirectoryChangesMonitor)));
                }
                Info("Monitoring {0} paths for files with extension(s) {1}.", Paths.Count,
                    Paths.Values.Distinct());
                return SetInitializedStatusAndReturnSucces();
            }
            catch (Exception e)
            {
                Error(e, "Error occurred initializing a detector.");
                return SetErrorStatusAndReturnFailure();
            }
        }

        public override ApiResult Shutdown()
        {
            if(base.Shutdown() != ApiResult.Success) return ApiResult.Failure;
            Debug("Disposing of {0} FileSystemWatchers.", Detectors.Count);
            foreach (FileSystemActivity fsa in Detectors)
            {
                fsa.Dispose();
            }
            return ApiResult.Success;
        }

        protected override ApiResult ProcessDetectorQueueMessage(FileSystemChangeMessage message)
        {
            string artifactName = string.Format("{0}_{1}", message.Id, Path.GetFileName(message.Path));
            string artifactPath = Profile.GetArtifactsDirectoryPathTo(artifactName);
            Debug("Waiting a bit for file to complete write...");
            Thread.Sleep(300);
            if (TryOpenFile(message.Path, out FileInfo f))
            {
                if (f.Length < 1024 * 1024 * 500)
                {
                    if (TryReadFile(message.Path, out byte[] data))
                    {
                        Debug("Read {0} bytes from {1}.", data.Length, message.Path);
                        File.WriteAllBytes(artifactPath, data);
                        Debug("Wrote {0} bytes to {1}.", data.Length, artifactPath);
                        Global.MessageQueue.Enqueue<DirectoryChangesMonitor>(
                            new FileArtifact(message.Id, artifactPath, data));
                        return ApiResult.Success;
                    }
                    else
                    {
                        Error("Could not read artifact data from {0}.", message.Path);
                        return ApiResult.Failure;
                    }
                }
                else
                {
                    if (TryCopyFile(message.Path, artifactPath))
                    {
                        Debug("Copied artifact {0} to {1}.", message.Path, artifactPath);
                        Global.MessageQueue.Enqueue<DirectoryChangesMonitor>(
                            new FileArtifact(message.Id, artifactPath));
                        return ApiResult.Success;
                    }
                    else
                    {
                        Error("Could not copy artifact {0} to {1}.", message.Path, artifactPath);
                        return ApiResult.Failure;
                    }
                }
            }
            else
            {
                Error("Could not open {0}.", message.Path);
                return ApiResult.Failure;
            }
            
        }

        protected bool TryCopyFile(string oldPath, string newPath, int maxTries = 100)
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
                    Debug("{0} file locked. Pausing a bit and then retrying copy ({1})...", oldPath, ++tries);
                    Thread.Sleep(50);
                }
                catch (Exception e)
                {
                    Error(e, "Unknown error attempting to copy {0}. Aborting.", oldPath);
                    return false;
                }
            }
            return false;
        }

        protected bool TryReadFile(string path, out byte[] data, int maxTries = 100)
        {
            int tries = 0;
            data = null;
            while (tries < maxTries)
            {
                try
                {
                    data = File.ReadAllBytes(path);
                    return true;
                }
                catch (IOException)
                {
                    Debug("{0} file locked. Pausing a bit and then retrying read ({1})...", path, ++tries);
                    Thread.Sleep(200);
                }
                catch (Exception e)
                {
                    data = null;
                    Error(e, "Unknown error attempting to read {0}. Aborting.", path);
                    return false;
                }
            }
            return false;
        }

        protected bool TryOpenFile(string path, out FileInfo file, int maxTries = 100)
        {
            file = null;
            int tries = 0;
            while (tries < maxTries)
            {
                try
                {
                    file = new FileInfo(path);
                    if (file.Exists)
                    {
                        return true;
                    }
                }
                catch (IOException)
                {
                    Debug("{0} file locked. Pausing a bit and then retrying open ({1})...", path, ++tries);
                    Thread.Sleep(50);
                }
                catch (Exception e)
                {
                    Error(e, "Unknown error attempting to read {0}. Aborting.", path);
                    return false;
                }
            }
            return false;
        }
        #endregion
    }
}
