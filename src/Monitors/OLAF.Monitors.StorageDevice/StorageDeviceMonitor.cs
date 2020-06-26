using System;

using OLAF.ActivityDetectors;

namespace OLAF.Monitors
{
    public class StorageDeviceMonitor : Monitor<StorageDeviceActivity, StorageDeviceActivityMessage, FileArtifact>
    {
        #region Constructors
        public StorageDeviceMonitor() : base() 
        {
            Detector = new StorageDeviceActivity(typeof(StorageDeviceMonitor));
            Status = ApiStatus.Initializing;
        }
        #endregion

        #region Overridden members
        public override ApiResult Init()
        {
            if (Status != ApiStatus.Initializing) return ApiResult.Failure;

            if ((Detector.Status == ApiStatus.Initialized))
            {
                Detectors.Add(Detector);
                return SetInitializedStatusAndReturnSucces();
            }
            else
            {
                Error("Storage device activity detector did not initialize.");
                return ApiResult.Failure;
            }

        }

        protected override ApiResult ProcessDetectorQueueMessage(StorageDeviceActivityMessage message)
        {
            if (message.EventType == StorageActivityEventType.Inserted)
            {
                Info("Storage device mounted at drive letter {0}.", message.DriveLetter);

            }
            else
            {
                Info("Storage device at drive letter {0} removed.", message.DriveLetter);
            }
            
            return ApiResult.Success;
        }
        #endregion

        #region Properties
        protected StorageDeviceActivity Detector { get; }
        #endregion
    }
}
