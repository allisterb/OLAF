using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Didac.Loggers
{
    public class SerilogLoggerException : LoggerException
    {
        public SerilogLogger SerilogLogger { get; }

        public SerilogLoggerException(SerilogLogger logger, string message) : base(logger, message)
        {
            SerilogLogger = logger;
        }
    }
}
