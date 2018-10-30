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

        public const int SRCCOPY = 0x00CC0020;

        public const int CAPTUREBLT = 0x40000000;

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

        #region Enums
        enum ShowWindowCommands
        {
            /// <summary>
            /// Hides the window and activates another window.
            /// </summary>
            Hide = 0,
            /// <summary>
            /// Activates and displays a window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when displaying the window 
            /// for the first time.
            /// </summary>
            Normal = 1,
            /// <summary>
            /// Activates the window and displays it as a minimized window.
            /// </summary>
            ShowMinimized = 2,
            /// <summary>
            /// Maximizes the specified window.
            /// </summary>
            Maximize = 3, // is this the right value?
                          /// <summary>
                          /// Activates the window and displays it as a maximized window.
                          /// </summary>       
            ShowMaximized = 3,
            /// <summary>
            /// Displays a window in its most recent size and position. This value 
            /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
            /// the window is not activated.
            /// </summary>
            ShowNoActivate = 4,
            /// <summary>
            /// Activates the window and displays it in its current size and position. 
            /// </summary>
            Show = 5,
            /// <summary>
            /// Minimizes the specified window and activates the next top-level 
            /// window in the Z order.
            /// </summary>
            Minimize = 6,
            /// <summary>
            /// Displays the window as a minimized window. This value is similar to
            /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowMinNoActive = 7,
            /// <summary>
            /// Displays the window in its current size and position. This value is 
            /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowNA = 8,
            /// <summary>
            /// Activates and displays the window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position. 
            /// An application should specify this flag when restoring a minimized window.
            /// </summary>
            Restore = 9,
            /// <summary>
            /// Sets the show state based on the SW_* value specified in the 
            /// STARTUPINFO structure passed to the CreateProcess function by the 
            /// program that started the application.
            /// </summary>
            ShowDefault = 10,
            /// <summary>
            ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
            /// that owns the window is not responding. This flag should only be 
            /// used when minimizing windows from a different thread.
            /// </summary>
            ForceMinimize = 11
        }
        #endregion

        #region Methods

        #region Files
        [DllImport("kernel32.dll")]
        public static extern IntPtr FindFirstFile(string pFileName, ref WIN32_FIND_DATA pFindFileData);

        [DllImport("kernel32.dll")]
        public static extern bool FindNextFile(IntPtr hFindFile, ref WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll")]
        public static extern bool FindClose(IntPtr hFindFile);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        #endregion

        #region Windows
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

        #region GDI
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        /// <summary>
        ///        Creates a memory device context (DC) compatible with the specified device.
        /// </summary>
        /// <param name="hdc">A handle to an existing DC. If this handle is NULL,
        ///        the function creates a memory DC compatible with the application's current screen.</param>
        /// <returns>
        ///        If the function succeeds, the return value is the handle to a memory DC.
        ///        If the function fails, the return value is <see cref="System.IntPtr.Zero"/>.
        /// </returns>
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC([In] IntPtr hdc);

        /// <summary>
        ///        Creates a bitmap compatible with the device that is associated with the specified device context.
        /// </summary>
        /// <param name="hdc">A handle to a device context.</param>
        /// <param name="nWidth">The bitmap width, in pixels.</param>
        /// <param name="nHeight">The bitmap height, in pixels.</param>
        /// <returns>If the function succeeds, the return value is a handle to the compatible bitmap (DDB). If the function fails, the return value is <see cref="System.IntPtr.Zero"/>.</returns>
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap([In] IntPtr hdc, int nWidth, int nHeight);

        /// <summary>Deletes the specified device context (DC).</summary>
        /// <param name="hdc">A handle to the device context.</param>
        /// <returns><para>If the function succeeds, the return value is nonzero.</para><para>If the function fails, the return value is zero.</para></returns>
        /// <remarks>An application must not delete a DC whose handle was obtained by calling the <c>GetDC</c> function. Instead, it must call the <c>ReleaseDC</c> function to free the DC.</remarks>
        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern bool DeleteDC([In] IntPtr hdc);

        /// <summary>Selects an object into the specified device context (DC). The new object replaces the previous object of the same type.</summary>
        /// <param name="hdc">A handle to the DC.</param>
        /// <param name="hgdiobj">A handle to the object to be selected.</param>
        /// <returns>
        ///   <para>If the selected object is not a region and the function succeeds, the return value is a handle to the object being replaced. If the selected object is a region and the function succeeds, the return value is one of the following values.</para>
        ///   <para>SIMPLEREGION - Region consists of a single rectangle.</para>
        ///   <para>COMPLEXREGION - Region consists of more than one rectangle.</para>
        ///   <para>NULLREGION - Region is empty.</para>
        ///   <para>If an error occurs and the selected object is not a region, the return value is <c>NULL</c>. Otherwise, it is <c>HGDI_ERROR</c>.</para>
        /// </returns>
        /// <remarks>
        ///   <para>This function returns the previously selected object of the specified type. An application should always replace a new object with the original, default object after it has finished drawing with the new object.</para>
        ///   <para>An application cannot select a single bitmap into more than one DC at a time.</para>
        ///   <para>ICM: If the object being selected is a brush or a pen, color management is performed.</para>
        /// </remarks>
        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject([In] IntPtr hdc, [In] IntPtr hgdiobj);

        /// <summary>Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system resources associated with the object. After the object is deleted, the specified handle is no longer valid.</summary>
        /// <param name="hObject">A handle to a logical pen, brush, font, bitmap, region, or palette.</param>
        /// <returns>
        ///   <para>If the function succeeds, the return value is nonzero.</para>
        ///   <para>If the specified handle is not valid or is currently selected into a DC, the return value is zero.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Do not delete a drawing object (pen or brush) while it is still selected into a DC.</para>
        ///   <para>When a pattern brush is deleted, the bitmap associated with the brush is not deleted. The bitmap must be deleted independently.</para>
        /// </remarks>
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        /// <summary>
        ///    Performs a bit-block transfer of the color data corresponding to a
        ///    rectangle of pixels from the specified source device context into
        ///    a destination device context.
        /// </summary>
        /// <param name="hdc">Handle to the destination device context.</param>
        /// <param name="nXDest">The leftmost x-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nYDest">The topmost y-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nWidth">The width of the source and destination rectangles (in pixels).</param>
        /// <param name="nHeight">The height of the source and the destination rectangles (in pixels).</param>
        /// <param name="hdcSrc">Handle to the source device context.</param>
        /// <param name="nXSrc">The leftmost x-coordinate of the source rectangle (in pixels).</param>
        /// <param name="nYSrc">The topmost y-coordinate of the source rectangle (in pixels).</param>
        /// <param name="dwRop">A raster-operation code.</param>
        /// <returns>
        ///    <c>true</c> if the operation succeedes, <c>false</c> otherwise. To get extended error information, call <see cref="System.Runtime.InteropServices.Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, 
            [In] IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);
        #endregion

        #endregion

        #region Event handlers
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool EnumWindowsProc(IntPtr hwnd, Int32 lParam);
        #endregion
    }
}
