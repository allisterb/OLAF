using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.Services.Storage
{
    public class BaseLogEvent
    {
        public string Type { get; set; }
        public string Severity { get; set; }

        public string ComputerName { get; set; } = System.Environment.MachineName;
        public string Message { get; set; }
        public string Source { get; set; }
        public string Category { get; set; }
    }
}
