using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.Win32
{
    [SuppressUnmanagedCodeSecurity]
    public class UnsafeNativeMethods
    {
        #region Constants
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public const int FILE_ATTRIBUTE_DIRECTORY = 16;

        public const int GWL_HWNDPARENT = -8;

        #endregion

        #region Structs
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct WIN32_FIND_DATA
        {
            public int dwFileAttributes;
            public int ftCreationTime_dwLowDateTime;
            public int ftCreationTime_dwHighDateTime;
            public int ftLastAccessTime_dwLowDateTime;
            public int ftLastAccessTime_dwHighDateTime;
            public int ftLastWriteTime_dwLowDateTime;
            public int ftLastWriteTime_dwHighDateTime;
            public int nFileSizeHigh;
            public int nFileSizeLow;
            public int dwReserved0;
            public int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        /// &lt;summary&gt;
        /// The RECT structure defines the coordinates of the upper-left and lower-right corners of a rectangle.
        /// &lt;/summary&gt;
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }

            public Rectangle AsRectangle
            {
                get
                {
                    return new Rectangle(this.Left, this.Top, this.Right - this.Left, this.Bottom - this.Top);
                }
            }

            public static RECT FromXYWH(int x, int y, int width, int height)
            {
                return new RECT(x, y, x + width, y + height);
            }

            public static RECT FromRectangle(Rectangle rect)
            {
                return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
            }
        }
        #endregion

        #region Methods
        [DllImport("kernel32.dll")]
        public static extern IntPtr FindFirstFile(string pFileName, ref WIN32_FIND_DATA pFindFileData);

        [DllImport("kernel32.dll")]
        public static extern bool FindNextFile(IntPtr hFindFile, ref WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll")]
        public static extern bool FindClose(IntPtr hFindFile);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);


        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern int EnumWindows(EnumWindowsProc lpEnumFunc, Int32 lParam);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Int32 GetWindowText(IntPtr hwnd,
            StringBuilder lpString, Int32 cch);

        [DllImport("user32.dll")]
        public static extern Int32 GetWindowTextLength(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern Int32 GetWindowLong(IntPtr hwnd, Int32 nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr intptr);
        #endregion

        #region Event handlers
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool EnumWindowsProc(IntPtr hwnd, Int32 lParam);
        #endregion
    }
}
