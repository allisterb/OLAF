using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OLAF.ActivityDetectors.Windows;

namespace OLAF.Monitors.Windows
{
    public class AppWindowMonitor : Monitor<AppWindowActivity, AppWindowActivityMessage, AppWindowArtifact>
    {
        public AppWindowMonitor(string processName) : base()
        {
            ProcessName = processName;
            AppWindowActivity = new AppWindowActivity(Type, processName, TimeSpan.FromMilliseconds(1000));
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
            message.Window.Save(GetLogDirectoryPathTo("{0}_bitmap{1}.bmp".F(ProcessName, message.Id)));
            EnqueueMessage(new AppWindowArtifact(message.Id, null,
                message.Window));
            return ApiResult.Success;
        }

        public string ProcessName { get; }

        public AppWindowActivity AppWindowActivity { get; }
    }
}
