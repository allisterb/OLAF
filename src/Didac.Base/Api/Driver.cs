using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Didac
{
    public static class Driver
    {
        public static ILogger Logger { get; private set; }

        public static void SetLogger(Func<ILogger> logger)
        {
            lock (setLogLock)
            {
                if (Logger == null)
                {
                    Logger = logger();
                }
            }
        }
        private static object setLogLock = new object();
    }
}
