using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OLAF.ActivityDetectors.Windows;

namespace OLAF.Monitors.Windows
{
    public class ExplorerMonitor : WindowsAppHookMonitor<FileActivityHook, FileActivityMessage, Message>
    {
        #region Constructors
        public ExplorerMonitor() : base("explorer")
        {

        }
        #endregion
    }
}
