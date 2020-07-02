using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
            UnsafeNativeMethods.GetClientRect(hwnd, ref rect);
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
            UnsafeNativeMethods.GetWindowRect(hwnd, ref rect);
            return rect.AsRectangle;
        }

        public static Rectangle GetAbsoluteClientRect(IntPtr hWnd)
        {
            Rectangle windowRect = Interop.GetWindowRect(hWnd);
            Rectangle clientRect = Interop.GetClientRect(hWnd);

            // This gives us the width of the left, right and bottom chrome - we can then determine the top height
            int chromeWidth = (int)((windowRect.Width - clientRect.Width) / 2);

            return new Rectangle(new System.Drawing.Point(windowRect.X + chromeWidth, windowRect.Y + (windowRect.Height - clientRect.Height - chromeWidth)), clientRect.Size);
        }

        public static Rectangle AdjustWindowRectangeToDesktopBounds(Rectangle win)
        {
            Rectangle d = GetAbsoluteClientRect(UnsafeNativeMethods.GetDesktopWindow());

            int x = win.X >= 0 ? win.X : 0;
            int w = win.X >= 0 ? win.Width : win.Width + (0 - win.X);

            int y = win.Y >= 0 ? win.Y : 0;
            int h = win.Y >= 0 ? win.Height : win.Height + (0 - win.Y);

            if (w + x > d.Width)
            {
                w = d.Width - x;
            }

            if (h + y > d.Height)
            {
                h = d.Height - y;
            }
            return new Rectangle(x, y, w, h);
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

        public static string GetWindowTitle(IntPtr hwnd)
        {
            StringBuilder title = new StringBuilder(GetWindowTextLength(hwnd) + 1);
            GetWindowText(hwnd, title, title.Capacity);
            return title.ToString();
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

        public static IntPtr GetSystemInformation(SYSTEM_INFORMATION_CLASS infoClass, out NTSTATUS result, uint infoLength = 0)
        {
            if (infoLength == 0)
            {
                infoLength = 0x10000;
            }
            IntPtr infoPtr = Marshal.AllocHGlobal((int)infoLength);

            int tries = 0;
            while (true)
            {
                result = NtQuerySystemInformation(infoClass, infoPtr, infoLength, out infoLength);

                if (result == NTSTATUS.SUCCESS)
                    return infoPtr;

                Marshal.FreeHGlobal(infoPtr);  //free pointer when not Successful

                if (result != NTSTATUS.INFO_LENGTH_MISMATCH && result != NTSTATUS.BUFFER_OVERFLOW && result != NTSTATUS.BUFFER_TOO_SMALL)
                {
                    return IntPtr.Zero;
                }
                else if (++tries > 5)
                {
                    return IntPtr.Zero;
                }
                else
                {
                    infoPtr = Marshal.AllocHGlobal((int)infoLength);
                }
            }
        }

        /// <summary>
        /// Gets drive letter from a bit mask where bit 0 = A, bit 1 = B etc.
        /// There can actually be more than one drive in the mask but we 
        /// just use the last one in this case.
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static char DriveMaskToLetter(int mask)
        {
            char letter;
            string drives = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            // 1 = A
            // 2 = B
            // 4 = C...
            int cnt = 0;
            int pom = mask / 2;
            while (pom != 0)
            {
                // while there is any bit set in the mask
                // shift it to the righ...                
                pom = pom / 2;
                cnt++;
            }

            if (cnt < drives.Length)
                letter = drives[cnt];
            else
                letter = '?';

            return letter;
        }

        /// <summary>
        /// Opens a directory, returns it's handle or zero.
        /// </summary>
        /// <param name="dirPath">path to the directory, e.g. "C:\\dir"</param>
        /// <returns>handle to the directory. Close it with CloseHandle().</returns>

        public static IntPtr OpenDirectory(string dirPath)
        {
            //
            // CreateFile  - MSDN
            const uint GENERIC_READ = 0x80000000;
            const uint OPEN_EXISTING = 3;
            const uint FILE_SHARE_READ = 0x00000001;
            const uint FILE_SHARE_WRITE = 0x00000002;
            const uint FILE_ATTRIBUTE_NORMAL = 128;
            const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
            IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
            // open the existing file for reading          

            IntPtr handle = CreateFile(
                  dirPath,
                  GENERIC_READ,
                  FILE_SHARE_READ | FILE_SHARE_WRITE,
                  0,
                  OPEN_EXISTING,
                  FILE_FLAG_BACKUP_SEMANTICS | FILE_ATTRIBUTE_NORMAL,
                  0);

            if (handle == INVALID_HANDLE_VALUE)
                return IntPtr.Zero;
            else
                return handle;
        }

        public static bool CloseDirectoryHandle(IntPtr handle)
        {
            return CloseHandle(handle);
        }
        /// <summary>
        /// New version which gets the handle automatically for specified directory
        /// Only for registering! Unregister with the old version of this function...
        /// </summary>
        /// <param name="register"></param>
        /// <param name="dirPath">e.g. C:\\dir</param>
        public static IntPtr RegisterDeviceNotification(IntPtr wndHandle, string dirPath)
        {
            IntPtr handle = OpenDirectory(dirPath);
            if (handle == IntPtr.Zero)
            {
                return IntPtr.Zero;
                
            }
            // save handle for closing it when unregistering
            
           
            DEV_BROADCAST_HANDLE data = new DEV_BROADCAST_HANDLE();
            data.dbch_devicetype = DBT_DEVTYP_HANDLE;
            data.dbch_reserved = 0;
            data.dbch_nameoffset = 0;
            //data.dbch_data = null;
            //data.dbch_eventguid = 0;
            data.dbch_handle = handle;
            data.dbch_hdevnotify = (IntPtr)0;
            int size = Marshal.SizeOf(data);
            data.dbch_size = size;
            IntPtr buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(data, buffer, true);
            return UnsafeNativeMethods.RegisterDeviceNotification(wndHandle, buffer, 0);
        }

        public static string GetCurrentWindowTitle()
        {
            IntPtr activeWindowHandle = GetForegroundWindow();
            return GetWindowTitle(activeWindowHandle);
        }
        private static object interopLock = new object();

    }
}
