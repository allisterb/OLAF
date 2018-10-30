using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.ActivityDetectors.Windows
{
    public class AppWindowActivityMessage : Message
    {
        public AppWindowActivityMessage(long id, string processName, Dictionary<Process, Bitmap> processWindows) : base(id)
        {
            ProcessName = processName ?? throw new ArgumentNullException(nameof(processName));
            ProcessWindows = processWindows ?? throw new ArgumentNullException(nameof(processWindows)); ;
            Window = processWindows.First().Value;
        }

        public AppWindowActivityMessage(long id, string processName, Bitmap processWindow) : base(id)
        {
            ProcessName = processName;

            Window = processWindow;
        }
        public string ProcessName { get; }

        public Bitmap Window { get; }

        public Dictionary<Process, Bitmap> ProcessWindows { get; }
    }
}
