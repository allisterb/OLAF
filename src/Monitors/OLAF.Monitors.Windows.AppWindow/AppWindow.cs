using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OLAF.ActivityDetectors;
using OLAF.ActivityDetectors.Windows;

namespace OLAF.Monitors.Windows
{
    public class AppWindowMonitor : Monitor<AppWindowActivity, AppWindowActivityMessage, AppWindowActivityMessage>
    {
        public AppWindowMonitor(string processName) : base()
        {
            ProcessName = processName;
            AppWindowActivity = new AppWindowActivity(Type, processName, TimeSpan.FromMilliseconds(500));
            if (AppWindowActivity.Status == ApiStatus.Ok)
            {
                Detectors.Add(AppWindowActivity);
                this.Status = ApiStatus.Initializing;
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
            //Global.MessageQueue.Enqueue<AppWindowMonitor>(message);
            return ApiResult.Success;
        }

        public string ProcessName { get; }
        public AppWindowActivity AppWindowActivity { get; }
    }
}
