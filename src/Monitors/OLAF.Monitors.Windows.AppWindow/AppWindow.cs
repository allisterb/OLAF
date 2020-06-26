using System;

using OLAF.ActivityDetectors.Windows;

namespace OLAF.Monitors.Windows
{
    public class AppWindowMonitor : Monitor<AppWindowActivity, AppWindowActivityMessage, AppWindowArtifact>
    {
        public AppWindowMonitor(string processName) : base()
        {
            ProcessName = processName;
            AppWindowActivity = new AppWindowActivity(Type, processName, TimeSpan.FromMilliseconds(5000));
            if (AppWindowActivity.Status == ApiStatus.Ok)
            {
                Detectors.Add(AppWindowActivity);
                this.Status = ApiStatus.Initializing;
                Info("Creating app window monitor for process {0}.", processName);
            }
            else
            {
                Error("Could not create AppWindowActivity detector for process {0}.", processName);
                this.Status = ApiStatus.Error;
            }
        }

        public override ApiResult Init()
        {
            if (AppWindowActivity.Enable() == ApiResult.Success)
            {
                return SetInitializedStatusAndReturnSucces();
            }
            else
            {
                Error("Could not enable AppWindowActivity detector for process {0}.", ProcessName);
                return SetErrorStatusAndReturnFailure();
            }

        }

        protected override ApiResult ProcessDetectorQueueMessage(AppWindowActivityMessage message)
        {
            Info("Analyzing app window of process {0} with dimensions {1}x{2}.", 
                message.ProcessName, message.Window.Width, message.Window.Height);
            EnqueueMessage(new AppWindowArtifact(message.ProcessName, message.Window));
            return ApiResult.Success;
        }

        public string ProcessName { get; }

        public AppWindowActivity AppWindowActivity { get; }
    }
}
