using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public class Monitor : OLAFApi<Monitor>
    {
        public List<Detector> Detectors { get; protected set; }
    }
}
