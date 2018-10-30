using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public class AppWindowArtifact : ImageArtifact
    {
        public AppWindowArtifact(long id, string processName, Bitmap image) : base(id, image)
        {
            ProcessName = processName;
        }
   
        public string ProcessName { get; }
    }
}
