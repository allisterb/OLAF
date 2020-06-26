using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Runtime.InteropServices;

using ManagedWinapi.Windows;
using ManagedWinapi.Windows.Contents;

using OLAF.Win32;
namespace OLAF.ActivityDetectors.Windows
{
    public class MSAAContent 
    {
        public ApiResult GetContent(IntPtr hwnd)
        {
            Rectangle rect = Interop.GetWindowRect(hwnd);
            SystemWindow w = SystemWindow.FromPoint(rect.X, rect.Y);
            WindowContent content = w.Content;
            return ApiResult.Unknown;
        }
    }
}
