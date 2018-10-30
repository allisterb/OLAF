using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OLAF.Win32;
namespace OLAF.ActivityDetectors.Windows
{
    public class GDICapture
    {
        public Bitmap CaptureWindow(IntPtr hwnd)
        {
            IntPtr desktopHwnd = UnsafeNativeMethods.GetDesktopWindow();
            IntPtr desktopDC = UnsafeNativeMethods.GetWindowDC(desktopHwnd);
            IntPtr wDC = UnsafeNativeMethods.GetWindowDC(hwnd);
            Rectangle windowRegion = Interop.GetAbsoluteClientRect(hwnd);
            IntPtr memoryDC = UnsafeNativeMethods.CreateCompatibleDC(desktopDC);
            IntPtr hBitmap = UnsafeNativeMethods.CreateCompatibleBitmap(desktopDC, windowRegion.Width, windowRegion.Height);
            IntPtr holdBitmap = UnsafeNativeMethods.SelectObject(memoryDC, hBitmap);
            Bitmap b = null;
            
            bool success = UnsafeNativeMethods.BitBlt
                (memoryDC, 
                0, 
                0, 
                windowRegion.Width, 
                windowRegion.Height, 
                desktopDC, 
                windowRegion.Left, 
                windowRegion.Top, 
                UnsafeNativeMethods.SRCCOPY | 
                UnsafeNativeMethods.CAPTUREBLT);
            if (success)
            {
                b = Image.FromHbitmap(hBitmap);
            }

            UnsafeNativeMethods.SelectObject(memoryDC, holdBitmap);
            UnsafeNativeMethods.DeleteObject(hBitmap);

            UnsafeNativeMethods.DeleteDC(memoryDC);
            UnsafeNativeMethods.ReleaseDC(desktopHwnd, desktopDC);
            UnsafeNativeMethods.ReleaseDC(hwnd, wDC);
            //UnsafeNativeMethods.Re

            return b;
        }
    }
}
