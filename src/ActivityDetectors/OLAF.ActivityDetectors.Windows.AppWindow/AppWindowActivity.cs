using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Timers;
using System.Buffers;
using System.Runtime.CompilerServices;

using System.Threading;

using SlimDX.Direct3D9;
using OLAF.Win32;

namespace OLAF.ActivityDetectors.Windows
{
    public class AppWindowActivity : ActivityDetector<AppWindowActivityMessage>
    {
        #region Constructors
        public AppWindowActivity(IMonitor monitor, Type monitorType, string processName, TimeSpan interval): base(monitor, monitorType)
        {
            this.processName = processName;
            D3D9Capture = new D3D9Capture();
            GDICapture = new GDICapture();
            Interval = interval;
            Timer = new System.Timers.Timer(Interval.TotalMilliseconds);
            Timer.AutoReset = true;
            Timer.Elapsed += Timer_Elapsed;
            previousCapture = new Bitmap(1, 1);
            Status = ApiStatus.Ok;
        }
        #endregion

        #region Overriden members
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
        #endregion

        #region Event handlers
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes == null || processes.Length == 0)
            {
                return;
            }

            Bitmap capture = null;
            IntPtr activeWindowHandle = UnsafeNativeMethods.GetForegroundWindow();
            string title = Interop.GetWindowTitle(activeWindowHandle);
            Rectangle rect = Interop.GetWindowRect(activeWindowHandle);
            int l = rect.Left, t = rect.Top;
            var tid = UnsafeNativeMethods.GetWindowThreadProcessId(activeWindowHandle, out uint pid);

            if (processes.Any(p => p.Id == pid))
            {
                Debug("{0} window detected at ({1},{2}).", processName, l, t);
                try
                {
                    capture = D3D9Capture.CaptureWindow(activeWindowHandle);
                }
                catch (Direct3D9Exception d3de)
                {
                    Debug("{0}. Falling back to GDI capture.", d3de.Message);
                    capture = GDICapture.CaptureWindow(activeWindowHandle);
                }
                if (capture == null)
                {
                    Error("Could not detect any window activity for app process {0}.", processName);
                    return;
                }
            }
            else if (processName == "chrome" && title.Contains("Google Chrome"))
            {
                Debug("Google Chrome incognito mode window detected at ({1},{2}).", processName, rect.Left, rect.Top);
                try
                {
                    capture = D3D9Capture.CaptureWindow(activeWindowHandle);
                }
                catch (Direct3D9Exception d3de)
                {
                    Debug("{0}. Falling back to GDI capture.", d3de.Message);
                    capture = GDICapture.CaptureWindow(activeWindowHandle);
                }
                if (capture == null)
                {
                    Error("Could not detect window activity for Google Chrome.");
                    return;
                }
            }            
            else if ((processName == "MicrosoftEdge" || processName == "MicrosoftEdgeCP") && 
                title.Contains("Microsoft Edge"))
            {
                Debug("Microsoft Edge window detected at ({1},{2}).", processName, rect.Left, rect.Top);
                try
                {
                    capture = D3D9Capture.CaptureWindow(activeWindowHandle);
                }
                catch (Direct3D9Exception d3de)
                {
                    Debug("{0}. Falling back to GDI capture.", d3de.Message);
                    capture = GDICapture.CaptureWindow(activeWindowHandle);
                }
                if (capture == null)
                {
                    Error("Could not detect window activity for Microsoft Edge.");
                    return;
                }
            }
            else
            {
                return;
            }

            if (!ImagesAreDuplicate(previousCapture, capture))
            {
                Interlocked.Exchange(ref previousCapture, capture);
                Global.MessageQueue.Enqueue<AppWindowActivity>(new AppWindowActivityMessage(processName, capture, title));
            }
            else
            {
                Debug("{0} window did not change.", processName);
            }

        }

        #endregion

        #region Methods
        public unsafe bool ImagesAreDuplicate(Bitmap b1, Bitmap b2)
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

        public unsafe int CompareImagesLinewise(Bitmap b1, Bitmap b2)
        {
            if ((b1 == null) || (b2 == null)) throw new ArgumentNullException("Parameter cannot be null");
            
            if (b1.Size != b2.Size)
            {
                return b2.Size.Height;
            }

            var bd1 = b1.LockBits(new Rectangle(new Point(0, 0), b1.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bd2 = b2.LockBits(new Rectangle(new Point(0, 0), b2.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            int r = 0;

            try
            {
                IntPtr bd1scan0 = bd1.Scan0;
                IntPtr bd2scan0 = bd2.Scan0;
                void* v1 = bd1scan0.ToPointer();
                void* v2 = bd2scan0.ToPointer();


                int stride = bd1.Stride;
         
                for (int i = 0; i < b1.Height; i++)
                {
                    void* p1 = Unsafe.Add<byte>(v1, i * stride);
                    void* p2 = Unsafe.Add<byte>(v2, i * stride);
                    ReadOnlySpan<byte> s1 = new ReadOnlySpan<byte>(p1, stride);
                    ReadOnlySpan<byte> s2 = new ReadOnlySpan<byte>(p2, stride);
                    if (s1.SequenceEqual(s2))
                    {
                        r++;
                    }
                }
            }
            finally
            {
                b1.UnlockBits(bd1);
                b2.UnlockBits(bd2);

            }
            return r;
        }
        #endregion

        #region Properties
        readonly string processName;

        System.Timers.Timer Timer { get; }

        TimeSpan Interval { get; }

        D3D9Capture D3D9Capture { get; }

        GDICapture GDICapture { get; }
        #endregion

        #region Fields
        Bitmap previousCapture;
        #endregion
    }
}
