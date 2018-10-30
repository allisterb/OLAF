using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Timers;
using Threading = System.Threading;

using OLAF.Win32;

namespace OLAF.ActivityDetectors.Windows
{
    public class AppWindowActivity : ActivityDetector<AppWindowActivityMessage>
    {
        public AppWindowActivity(Type monitorType, string processName, TimeSpan interval): base(monitorType)
        {
            ProcessName = processName;
            CaptureDevice = new D3D9CaptureDevice();
            Interval = interval;
            Timer = new Timer(Interval.TotalMilliseconds);
            Timer.AutoReset = true;
            Timer.Elapsed += Timer_Elapsed;
            Status = ApiStatus.Ok;
        }

        public override ApiResult Enable()
        {
            if (Status != ApiStatus.Ok)
            {
                return ApiResult.Failure;
            }
            Timer.Enabled = true;
            Timer.Start();
            return ApiResult.Success;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        { 
            Process[] processes = Process.GetProcessesByName(ProcessName);
            if (processes == null || processes.Length == 0)
            {
                return;
            }
            IntPtr activeWindowHandle = UnsafeNativeMethods.GetForegroundWindow();
            var tid = UnsafeNativeMethods.GetWindowThreadProcessId(activeWindowHandle, out uint pid);
            if (processes.Any(p => p.Id == pid))
            {
                Dictionary<Process, Bitmap> processWindows =
                    processes
                    .Where(p => p.Id == pid)
                    .ToDictionary(p => p, p => CaptureDevice.CaptureWindow(p.MainWindowHandle));


                Debug("{0} process window in the foreground and will be captured.", ProcessName);

            Global.MessageQueue.Enqueue<AppWindowActivity>(
                new AppWindowActivityMessage(Threading.Interlocked.Increment(ref currentArtifactId),
                ProcessName, processWindows));
            }
        }

        string ProcessName { get; }

        Timer Timer { get; }

        TimeSpan Interval { get; }

        D3D9CaptureDevice CaptureDevice { get; }
    }
}
