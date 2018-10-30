using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.ActivityDetectors
{
    public class AppWindowActivityMessage : Message
    {
        public AppWindowActivityMessage(long id, string processName, Dictionary<Process, Bitmap> processWindows) : base(id)
        {
            ProcessName = processName;
            ProcessWindows = processWindows;
        }

        public string ProcessName { get; }

        public Dictionary<Process, Bitmap> ProcessWindows { get; }
    }
}
