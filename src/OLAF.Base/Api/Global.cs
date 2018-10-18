using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            MonitorAssemblies = Assembly.GetExecutingAssembly().LoadAllFrom("OLAF.Monitors.*.dll");
            foreach (string n in ExcludedAssemblyNames)
            {
                if (MonitorAssemblies.Any(a => a.FullName.StartsWith(n)))
                {
                    MonitorAssemblies.RemoveAll(a => a.FullName.StartsWith(n));
                }
            }
            if (MonitorAssemblies == null)
            {
                throw new Exception("Did not load any OLAF monitor assemblies.");
            }
            CancellationTokenSource = new CancellationTokenSource();
        }
        #endregion

        #region Properties
        public static ILogger Logger { get; private set; }

        public static List<Assembly> MonitorAssemblies { get; internal set; }

        public static List<Assembly> OLAFLoadedAssemblies { get; internal set; }

        public static string[] ExcludedAssemblyNames { get; } = new string[0];

        public static CancellationTokenSource CancellationTokenSource { get; }


        public static MessageQueue MessageQueue { get; private set; }
        #endregion

        #region Methods
        public static void SetupLogger(Func<ILogger> logger)
        {
            lock (setupLoggerLock)
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

        public static void SetupMessageQueue()
        {
            lock (setupMessageQueueLock)
            {
                if (MessageQueue == null)
                {
                    MessageQueue = new MessageQueue(GetSubTypes<Monitor>());
                }
            }
        }

        public static Type[] GetSubTypes<T>(string assemblyName = "")
        {
            IEnumerable<Assembly> assemblies = MonitorAssemblies;
            if (MonitorAssemblies.Count(a => assemblyName.IsNotEmpty() && a.GetName().Name == assemblyName) > 0)
            {
                assemblies = MonitorAssemblies.Where(a => a.FullName.StartsWith(assemblyName));
            }
            else if (assemblyName.IsNotEmpty())
            {
                return null;
            }

            return assemblies
                 .Select(a => a.GetTypes())
                 .SelectMany(t => t)
                 .Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract)?
                 .ToArray();
        }
        #endregion
        
        #region Fields
        private static object setupLoggerLock = new object();
        private static object setupMessageQueueLock = new object();
        #endregion
    }
}
