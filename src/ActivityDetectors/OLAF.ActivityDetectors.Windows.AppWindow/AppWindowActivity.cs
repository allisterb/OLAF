using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Timers;
using System.Buffers;

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
            appWindowDetectoMutex = new Threading.Mutex();
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

            appWindowDetectoMutex.WaitOne();

            Bitmap capture = null;
            IntPtr activeWindowHandle = UnsafeNativeMethods.GetForegroundWindow();
            var tid = UnsafeNativeMethods.GetWindowThreadProcessId(activeWindowHandle, out uint pid);

            if (processes.Any(p => p.Id == pid))
            {
                Debug("{0} process window in the foreground and will be captured.", ProcessName);
                try
                {
                    capture = D3D9Capture.CaptureWindow(activeWindowHandle);
                }
                catch (Direct3D9Exception)
                {
                    Debug("Falling back to GDI capture.");
                    capture = GDICapture.CaptureWindow(activeWindowHandle);
                }
                if (capture == null)
                {
                    Error("Could not detect any window activity for app process {0}.", ProcessName);
                }
            }
            else if (ProcessName == "chrome" && Interop.GetWindowTitle(activeWindowHandle).Contains("Google Chrome"))
            {
                Debug("Google Chrome incognito mode window detected.", ProcessName);
                try
                {
                    capture = D3D9Capture.CaptureWindow(activeWindowHandle);
                }
                catch (Direct3D9Exception)
                {
                    Debug("Falling back to GDI capture.");
                    capture = GDICapture.CaptureWindow(activeWindowHandle);
                }
                if (capture == null)
                {
                    Error("Could not detect any window activity for app process {0}.", ProcessName);
                }
            }            

            else if ((ProcessName == "MicrosoftEdge" || ProcessName == "MicrosoftEdgeCP") && 
                Interop.GetWindowTitle(activeWindowHandle).Contains("Microsoft Edge"))
            {
                Debug("Microsoft Edge window detected.", ProcessName);
                try
                {
                    capture = D3D9Capture.CaptureWindow(activeWindowHandle);
                }
                catch (Direct3D9Exception)
                {
                    Debug("Falling back to GDI capture.");
                    capture = GDICapture.CaptureWindow(activeWindowHandle);
                }
                if (capture == null)
                {
                    Error("Could not detect any window activity for app process {0}.", ProcessName);
                }
            }

            
            if (capture != null)
            {
                if (!CompareImages(previousCapture, capture))
                {
                    Global.MessageQueue.Enqueue<AppWindowActivity>(
                           new AppWindowActivityMessage(
                               Threading.Interlocked.Increment(ref currentArtifactId), ProcessName, capture));
                    previousCapture = capture;
                }
                else
                {
                    Debug("Skipping duplicate image");
                }
              
                appWindowDetectoMutex.ReleaseMutex();
            }
        }

        public unsafe bool CompareImages(Bitmap b1, Bitmap b2)
        {
            if ((b1 == null) != (b2 == null)) return false;
            if (b1.Size != b2.Size) return false;

            var bd1 = b1.LockBits(new Rectangle(new Point(0, 0), b1.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bd2 = b2.LockBits(new Rectangle(new Point(0, 0), b2.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);


            bool r = false;

            try
            {
                IntPtr bd1scan0 = bd1.Scan0;
                IntPtr bd2scan0 = bd2.Scan0;

                int stride = bd1.Stride;
                int len = stride * b1.Height;

                ReadOnlySpan<byte> s1 = new ReadOnlySpan<byte>(bd1scan0.ToPointer(), len);
                ReadOnlySpan<byte> s2 = new ReadOnlySpan<byte>(bd2scan0.ToPointer(), len);
                r = s1.SequenceEqual(s2);
            }
            finally
            {
                b1.UnlockBits(bd1);
                b2.UnlockBits(bd2);

            }
            return r;
        }

        string ProcessName { get; }

        Timer Timer { get; }

        TimeSpan Interval { get; }

        D3D9Capture D3D9Capture { get; }

        GDICapture GDICapture { get; }

        Bitmap previousCapture;

        object appWindowDetectorLock = new object();

        Threading.Mutex appWindowDetectoMutex;
    }
}
