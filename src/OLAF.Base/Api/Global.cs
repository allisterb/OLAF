using System;
using System.Collections.Generic;
using System.IO;
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
            MonitorAssemblies = Assembly.GetExecutingAssembly().LoadAllFrom("OLAF.Monitors.*.dll") ??
                throw new Exception("No monitor assemblies found in directory: " + AssemblyDirectory.FullName + ".");
            ActivityDetectorAssemblies = Assembly.GetExecutingAssembly().LoadAllFrom("OLAF.ActivityDetectors.*.dll") ??
                throw new Exception("No activity detector assemblies found in directory: " + AssemblyDirectory.FullName + ".");
            LoadedAssemblies = MonitorAssemblies.Concat(ActivityDetectorAssemblies).ToList();
            CancellationTokenSource = new CancellationTokenSource();
        }
        #endregion

        #region Properties
        public static ILogger Logger { get; private set; } = new SimpleConsoleLogger();

        private static DirectoryInfo AssemblyDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

        private static Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

        public static List<Assembly> MonitorAssemblies { get; internal set; }

        public static List<Assembly> ActivityDetectorAssemblies { get; internal set; }

        public static string[] ExcludedAssemblyNames { get; } = new string[0];

        public static List<Assembly> LoadedAssemblies { get; internal set; }

        public static CancellationTokenSource CancellationTokenSource { get; }

        public static MessageQueue MessageQueue { get; private set; }
        #endregion

        #region Methods
        public static void SetupLogger(Func<ILogger> logger)
        {
            lock (setupLoggerLock)
            {
                if (!loggerIsSetup)
                {
                    Logger = logger();
                    loggerIsSetup = true;
                }
                else
                {
                    throw new InvalidOperationException("The global logger is already configured.");
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
                    MessageQueue = new MessageQueue(GetSubTypes<Monitor>().Concat(GetSubTypes<ActivityDetector>())
                        .ToArray());
                }
            }
        }

        public static Type[] GetSubTypes<T>(string assemblyName = "")
        {
            IEnumerable<Assembly> assemblies = LoadedAssemblies;
            if (LoadedAssemblies.Count(a => assemblyName.IsNotEmpty() && a.GetName().Name == assemblyName) > 0)
            {
                assemblies = LoadedAssemblies.Where(a => a.FullName.StartsWith(assemblyName));
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
        private static bool loggerIsSetup = false;
        private static object setupMessageQueueLock = new object();
        #endregion
    }
}
