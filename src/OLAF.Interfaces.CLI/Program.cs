using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OLAF.Loggers;
using OLAF.Monitors.Windows;

namespace OLAF
{
    class Program : Interface
    {
        public static bool WithLogFile { get; set; } = false;
        public static bool WithoutConsole { get; set; } = false;
        public static bool WithDebugOutput { get; set; } = false;
        public static OSHookMonitor Monitor { get; protected set; }
        protected static ILogger Logger;

        static Program() {}

        static void Main(string[] args)
        {
            if (args.Contains("--wait-for-attach"))
            {
                Console.WriteLine("Attach debugger and press any key to continue execution...");
                Console.ReadKey(true);
                if (!Debugger.IsAttached)
                {
                    Console.WriteLine("No debugger detected! Exiting.");
                    return;
                }
                else
                {
                    Debugger.Break();
                }
            }

            List<string> enabledLogOptions = new List<string>();
            if (args.Contains("--with-log-file"))
            {
                WithLogFile = true;
                enabledLogOptions.Add("WithLogFile");
            }

            if (args.Contains("--with-debug"))
            {
                WithDebugOutput = true;
                enabledLogOptions.Add("WithDebugOutput");
            }

            if (args.Contains("--without-console"))
            {
                WithoutConsole = true;
                enabledLogOptions.Add("WithoutConsole");
            }

            Global.SetLogger(() => SerilogLogger.CreateLogger(enabledLogOptions));
            Logger = Global.Logger;

            Global.SetupHookMessageQueue();
            var processes = GetCurrentProcesses();
            Monitor = new EasyHookMonitor(
                GetCurrentProcesses().First(p => p.Value == "explorer").Key,
                "OLAF.Hooks.Windows.FileActions.dll");
            if (!Monitor.Initialized)
            {
                Logger.Error("Could not initialize hook monitor.");
                Exit(ExitCode.UnhandledException);
            }
            Monitor.Inject();
            while(true)
            {

            }
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Logger.Info("Stop requested by user.");
            Exit(ExitCode.Successsd);
        }

        static void Exit(ExitCode result)
        {
            Logger.Close();
            Environment.Exit((int)result);
        }
    }
}
