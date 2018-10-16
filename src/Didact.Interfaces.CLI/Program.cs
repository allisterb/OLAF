using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Didact.Loggers;
using Didact.Monitors.Windows;

namespace Didact
{

    class Program : UserInterface
    {
        public static bool WithLogFile { get; set; } = false;
        public static string LogFileName { get; set; }
        public static bool WithoutConsole { get; set; } = false;
        public static bool WithDebugOutput { get; set; } = false;
        public static OSHookMonitor Monitor { get; protected set; }

        protected static ILogger LL;

        static Program()
        {
            
        }

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

            if (args.Contains("--with-debug"))
            {
                WithDebugOutput = true;
            }

            if (args.Contains("--with-log-file"))
            {
                WithLogFile = true;
            }

            if (args.Contains("--without-console"))
            {
                WithoutConsole = true;
            }

            Global.SetLogger(() => SerilogLogger.CreateDefaultLogger("Didact.log"));
            Global.SetupHookMessageQueue();

            var processes = GetCurrentProcesses();
            Monitor = new EasyHookMonitor(
                GetCurrentProcesses().First(p => p.Value == "chrome").Key,
                "Didact.Hooks.Windows.FileActions.dll");
            if (!Monitor.Initialized)
            {
                LL.Error("Could not initialize hook monitor.");
                Exit(ExitCode.UnhandledException);
            }
            Monitor.Inject();
            while(true)
            {

            }
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            LL.Info("Stop requested by user.");
            Exit(ExitCode.Successsd);
        }

        static void Exit(ExitCode result)
        {
            LL.Close();
            Environment.Exit((int)result);
        }
    }
}