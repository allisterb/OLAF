using System;
using System.Collections.Generic;
using System.Drawing;

using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Timers;
using Threading = System.Threading;

using SlimDX.Direct3D9;
using OLAF.Win32;

namespace OLAF.ActivityDetectors.Windows
{
    public class AppWindowActivity : ActivityDetector<AppWindowActivityMessage>
    {
        public AppWindowActivity(Type monitorType, string processName, TimeSpan interval): base(monitorType)
        {
            ProcessName = processName;
            D3D9Capture = new D3D9Capture();
            GDICapture = new GDICapture();
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
            Bitmap capture;
            IntPtr activeWindowHandle = UnsafeNativeMethods.GetForegroundWindow();
            var tid = UnsafeNativeMethods.GetWindowThreadProcessId(activeWindowHandle, out uint pid);
            if (processes.Any(p => p.Id == pid))
            {
                Debug("{0} process window in the foreground and will be captured.", ProcessName);
                try
                {
                    capture = D3D9Capture.CaptureWindow(activeWindowHandle);
                }
                catch(Direct3D9Exception)
                {
                    Debug("Falling back to GDI capture.");
                    capture = GDICapture.CaptureWindow(activeWindowHandle);
                }
                
                Global.MessageQueue.Enqueue<AppWindowActivity>(
                         new AppWindowActivityMessage(
                             Threading.Interlocked.Increment(ref currentArtifactId), ProcessName, capture));
                

            }
            else if (ProcessName == "chrome" && Interop.GetWindowTitle(activeWindowHandle).Contains("Google Chrome"))
            {
                Debug("{0} process window in the foreground and will be captured.", ProcessName);
                try
                {
                    capture = D3D9Capture.CaptureWindow(activeWindowHandle);
                }
                catch (Exception)
                {
                    Debug("Falling back to GDI capture.");
                    capture = GDICapture.CaptureWindow(activeWindowHandle);
                }
                Global.MessageQueue.Enqueue<AppWindowActivity>(
                        new AppWindowActivityMessage(
                            Threading.Interlocked.Increment(ref currentArtifactId), ProcessName, capture));
            }
        }

        string ProcessName { get; }

        Timer Timer { get; }

        TimeSpan Interval { get; }

        D3D9Capture D3D9Capture { get; }

        GDICapture GDICapture { get; }
    }
}
