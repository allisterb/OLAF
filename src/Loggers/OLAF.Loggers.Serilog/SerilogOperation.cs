using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serilog;
using SerilogTimings;
using SerilogTimings.Extensions;

namespace OLAF.Loggers
{
    public class SerilogOperation : IOperationContext
    {
        Operation Operation { get; }

        public SerilogOperation(Operation op)
        {
            Operation = op;
        }

        public IDisposable Begin(string messageTemplate, params object[] arg) => Operation.Begin(messageTemplate, arg);

        public void Complete() => Operation.Complete();

        public void Cancel() => Operation.Cancel();

        public void Dispose() => Operation.Dispose();
    }
}
