using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Didact
{
    public abstract class Interface : DidactApi<Interface>
    {
        public static Dictionary<int, string> GetCurrentProcesses()
        {
            return Process.GetProcesses()
                .Select(p => new KeyValuePair<int, string>(p.Id, p.ProcessName))
                .ToDictionary(p => p.Key, p => p.Value);
        }
    }
}
