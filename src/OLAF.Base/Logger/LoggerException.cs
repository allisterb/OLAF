using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class LoggerException : Exception
    {
        public ILogger Logger { get;  }

        public LoggerException(ILogger logger, string message) : base(message)
        {
            Logger = logger;
        }
    }
}
