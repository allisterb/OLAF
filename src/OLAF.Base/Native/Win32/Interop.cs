using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static OLAF.Win32.UnsafeNativeMethods;

namespace OLAF.Win32
{
    public static class Interop
    {
        public static Dictionary<IntPtr, string> ActiveWindowsList { get; set; }
        
        
        /// <summary></summary>
        /// Get a windows client rectangle in a .NET structure
        /// </summary>
        /// <param name=&quot;hwnd&quot;>The window handle to look up</param>
        /// <returns>The rectangle</returns>
        public static Rectangle GetClientRect(IntPtr hwnd)
        {
            RECT rect = new RECT();
            UnsafeNativeMethods.GetClientRect(hwnd, out rect);
            return rect.AsRectangle;
        }

        /// <summary>
        /// Get a windows rectangle in a .NET structure
        /// </summary>
        /// <param name=&quot;hwnd&quot;>The window handle to look up</param>
        /// <returns>The rectangle</returns>
        public static Rectangle GetWindowRect(IntPtr hwnd)
        {
            RECT rect = new RECT();
            UnsafeNativeMethods.GetWindowRect(hwnd, out rect);
            return rect.AsRectangle;
        }

        public static Rectangle GetAbsoluteClientRect(IntPtr hWnd)
        {
            Rectangle windowRect = Interop.GetWindowRect(hWnd);
            Rectangle clientRect = Interop.GetClientRect(hWnd);

            // This gives us the width of the left, right and bottom chrome - we can then determine the top height
            int chromeWidth = (int)((windowRect.Width - clientRect.Width) / 2);

            return new Rectangle(new Point(windowRect.X + chromeWidth, windowRect.Y + (windowRect.Height - clientRect.Height - chromeWidth)), clientRect.Size);
        }

        public static Dictionary<IntPtr, string> GetActiveWindowsList()
        {
            lock (interopLock)
            {
                ActiveWindowsList = new Dictionary<IntPtr, string>();
                UnsafeNativeMethods.EnumWindows(EnumWinTitles, 1);
            }
            return ActiveWindowsList;
        }

        private static bool EnumWinTitles(IntPtr hwnd, Int32 lParam)
        {
            if (IsWindowVisible(hwnd))
            {
                if (GetParent(hwnd) == IntPtr.Zero)
                {
                    if (GetWindowLong(hwnd, GWL_HWNDPARENT) == 0)
                    {
                      
                        StringBuilder title = new StringBuilder(GetWindowTextLength(hwnd) + 1);
                        GetWindowText(hwnd, title, title.Capacity);

                        if (title.Length > 0)
                        {
                            ActiveWindowsList.Add(hwnd, title.ToString());
                        }
                    }
                }
            }
            return true;
        }

        private static object interopLock = new object(); 
    }
}
