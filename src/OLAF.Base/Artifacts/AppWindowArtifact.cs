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
        public AppWindowArtifact(long id, Process process, Bitmap image) : base(id, image)
        {
            Process = process;
        }

        public Process Process { get; }
    }
}
