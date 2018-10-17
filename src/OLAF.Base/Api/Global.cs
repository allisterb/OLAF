using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OLAF
{
    public static class Global
    {
        #region Constructor
        static Global()
        {
            CancellationTokenSource = new CancellationTokenSource();
        }
        #endregion

        #region Properties
        public static ILogger Logger { get; private set; }

        public static CancellationTokenSource CancellationTokenSource { get; }

        public static MessageQueue HookMessageQueue { get; private set; }
        #endregion

        #region Methods
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

        public static void CloseLogger()
        {
            if (Logger == null)
            {
                throw new InvalidOperationException("No logger is assigned.");
            }
            Logger.Close();
        }

        public static void SetupHookMessageQueue()
        {
            HookMessageQueue = new MessageQueue(CancellationTokenSource.Token);
        }

        #endregion

        #region Fields
        private static object setLogLock = new object();
        #endregion
    }
}
