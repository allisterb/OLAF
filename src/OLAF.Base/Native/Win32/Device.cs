#region Attribution
/**
 * contains code from Jan Dolinay - "Detecting USB Drive Removal in a C# Program"
 * https://www.codeproject.com/Articles/18062/Detecting-USB-Drive-Removal-in-a-C-Program
 * 
 * */
#endregion

using System;
using System.Runtime.InteropServices;   
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace OLAF.Win32
{
    // Structure with information for RegisterDeviceNotification.
    [StructLayout(LayoutKind.Sequential)]
    public struct DEV_BROADCAST_HANDLE
    {
        public int dbch_size;
        public int dbch_devicetype;
        public int dbch_reserved;
        public IntPtr dbch_handle;
        public IntPtr dbch_hdevnotify;
        public Guid dbch_eventguid;
        public long dbch_nameoffset;
        //public byte[] dbch_data[1]; // = new byte[1];
        public byte dbch_data;
        public byte dbch_data1;
    }

    // Struct for parameters of the WM_DEVICECHANGE message
    [StructLayout(LayoutKind.Sequential)]
    public struct DEV_BROADCAST_VOLUME
    {
        public int dbcv_size;
        public int dbcv_devicetype;
        public int dbcv_reserved;
        public int dbcv_unitmask;
    }
    public partial class UnsafeNativeMethods
    {
        //   HDEVNOTIFY RegisterDeviceNotification(HANDLE hRecipient,LPVOID NotificationFilter,DWORD Flags);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, uint Flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint UnregisterDeviceNotification(IntPtr hHandle);

        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        // should be "static extern unsafe"
        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr CreateFile(
              string FileName,                    // file name
              uint DesiredAccess,                 // access mode
              uint ShareMode,                     // share mode
              uint SecurityAttributes,            // Security Attributes
              uint CreationDisposition,           // how to create
              uint FlagsAndAttributes,            // file attributes
              int hTemplateFile                   // handle to template file
              );


        [DllImport("kernel32", SetLastError = true)]
        public static extern bool CloseHandle(
              IntPtr hObject   // handle to object
              );
        public static readonly int DBT_DEVTYP_DEVICEINTERFACE = 5;
        public static readonly int DBT_DEVTYP_HANDLE = 6;
        public static readonly int BROADCAST_QUERY_DENY = 0x424D5144;
        public static readonly int WM_DEVICECHANGE = 0x0219;
        public static readonly int DBT_DEVICEARRIVAL = 0x8000; // system detected a new device
        public static readonly int DBT_DEVICEQUERYREMOVE = 0x8001;   // Preparing to remove (any program can disable the removal)
        public static readonly int DBT_DEVICEREMOVECOMPLETE = 0x8004; // removed 
        public static readonly int DBT_DEVTYP_VOLUME = 0x00000002; // drive type is logical volume

    }
}
