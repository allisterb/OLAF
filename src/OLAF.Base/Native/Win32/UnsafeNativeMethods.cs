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
    public partial class UnsafeNativeMethods
    {
        #region Constants
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public const int GWL_HWNDPARENT = -8;       
        #endregion
    }
}
