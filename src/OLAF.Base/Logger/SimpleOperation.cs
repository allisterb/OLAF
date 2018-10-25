using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

namespace OLAF.Loggers
{
    public class SimpleOperation : IOperationContext
    {
        ILogger Logger;
        Stopwatch Watch { get; set; }

        public SimpleOperation(ILogger logger)
        {
            Logger = logger;
            Watch = new Stopwatch();
    
        }

        public IOperationContext Begin(string messageTemplate, params object[] arg)
        {
            Watch.Start();
            return this;
        }

        public void Cancel() => Watch.Stop();

        public void Complete() => Watch.Stop();

        public void Dispose()
        {
            Watch.Stop();

        }
    }
}
