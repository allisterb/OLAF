using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

using OLAF.Win32;

using OLAF.ActivityDetectors;

namespace OLAF.Monitors
{
    public class StorageDeviceMonitor : Monitor<StorageDeviceActivity, StorageDeviceActivityMessage, FileArtifact>
    {
        #region Constructors
        public StorageDeviceMonitor(string[] extensions, Profile profile) : base()
        {
            Extensions = extensions;
            Detector = new StorageDeviceActivity(typeof(StorageDeviceMonitor));
            Status = ApiStatus.Initializing;
        }
        #endregion

        #region Overridden members
        public override ApiResult Init()
        {
            if (Status != ApiStatus.Initializing) return ApiResult.Failure;

            if (Detector.Status == ApiStatus.Initialized)
            {
                Detectors.Add(Detector);
                return SetInitializedStatusAndReturnSucces();
            }
            else
            {
                Error("Storage device activity detector did not initialize.");
                return SetErrorStatusAndReturnFailure();
            }
        }

        public override ApiResult Shutdown()
        {
            if (base.Shutdown() != ApiResult.Success) return ApiResult.Failure;
            Debug("Disposing of {0}.", "StorageDeviceActivityDetector");
            Detector.Dispose();
            if (DirectoryChangesMonitors.Count != 0)
            {
                foreach (var dcm in DirectoryChangesMonitors)
                {
                    if (!dcm.Value.ShutdownRequested)
                    {
                        var r = dcm.Value.Shutdown();
                        if (r != ApiResult.Success)
                        {
                            Error("Shutdown of directory changes monitor for path {0} returned {1}.", dcm.Key, r);
                        }
                        else
                        {
                            Info("{0} for path {1} shutdown completed successfully.", "DirectoryChangeMonitor", dcm.Key);
                        }
                    }
                    dcm.Value.Dispose();
                }
                return ApiResult.Success;
            }
            else
            {
                Info("No {0} monitors active.", "DirectoryChangesMonitor");
                return ApiResult.Success;
            }


        }

        protected override ApiResult ProcessDetectorQueueMessage(StorageDeviceActivityMessage message)
        {
            if (message.EventType == StorageActivityEventType.Inserted)
            {
                Info("Storage device mounted at drive letter {0}.", message.DriveLetter);
                var path = message.DriveLetter + "\\";
                var m = new DirectoryChangesMonitor(new string[] { path }, Extensions, this.Profile, UserFileOperation.COPY_EXTERNAL);
                if (m.Init() == ApiResult.Failure)
                {
                    m.Dispose();
                    Error("Could not initialize {0} monitor for path {1}.", "DirectoryChangesMonitor", path);
                    return ApiResult.Failure;
                }
                else if (m.Start() == ApiResult.Failure)
                {
                    m.Dispose();
                    Error("Could not start {0} monitor for path {1}.", "DirectoryChangesMonitor", path);
                    return ApiResult.Failure;
                }
                else
                {
                    if (DirectoryChangesMonitors.TryAdd(path, m))
                    {
                        return ApiResult.Success;
                    }
                    else
                    {
                        Error("Could not add {0} for path {1}.", "DirectoryChangesMonitor", path);
                        m.Dispose();
                        return ApiResult.Failure;
                    }
                }
            }
            else
            {
                Info("Storage device at drive letter {0} removed.", message.DriveLetter);
                var path = message.DriveLetter + "\\";
                if (DirectoryChangesMonitors.ContainsKey(path))
                {
                    if (DirectoryChangesMonitors.TryRemove(path, out DirectoryChangesMonitor m))
                    {
                        m.Dispose();
                        Info("Stopped directory changes monitor for path {0}.", path);
                    }
                    else
                    {
                        Warn("Did not find {0} for path {1}, ignoring.", "DirectoryChangesMonitor", path);
                    }
                }
            }
            return ApiResult.Success;
        }
        #endregion

        #region Properties
        protected string[] Extensions { get; }
        protected StorageDeviceActivity Detector { get; }
        protected ConcurrentDictionary<string, DirectoryChangesMonitor> DirectoryChangesMonitors { get; } = new ConcurrentDictionary<string, DirectoryChangesMonitor>();
        protected Thread Win32MessageThread { get; set; }
        #endregion

    }
}
