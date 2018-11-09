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
        public AppWindowActivityMessage(string processName, Dictionary<Process, Bitmap> processWindows) : base()
        {
            ProcessName = processName ?? throw new ArgumentNullException(nameof(processName));
            ProcessWindows = processWindows ?? throw new ArgumentNullException(nameof(processWindows)); ;
            Window = processWindows.First().Value;
        }

        public AppWindowActivityMessage(string processName, Bitmap processWindow, string windowTitle) : base()
        {
            ProcessName = processName;

            Window = processWindow;

            WindowTitle = windowTitle;
        }
        public string ProcessName { get; }

        public Bitmap Window { get; }

        public string WindowTitle { get; }

        public Dictionary<Process, Bitmap> ProcessWindows { get; }
    }
}
