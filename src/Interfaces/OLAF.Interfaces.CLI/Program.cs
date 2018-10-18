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
        public static AppHookMonitor Monitor { get; protected set; }
       
        static Program()
        {
            AppDomain.CurrentDomain.UnhandledException += Program_UnhandledException;

            Console.CancelKeyPress += Console_CancelKeyPress;
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

            Global.SetupLogger(() => SerilogLogger.CreateLogger(enabledLogOptions));

            Global.SetupMessageQueue();
            var processes = GetCurrentProcesses();
            Monitor = new EasyHookMonitor(
                GetCurrentProcesses().First(p => p.Value == "explorer").Key);
            if (!Monitor.Initialized)
            {
                L.Error("Could not initialize hook monitor.");
                Exit(ExitCode.UnhandledException);
            }
            Monitor.Inject();
            while(true)
            {

            }
        }

        static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Global.CancellationTokenSource != null)
            {
                Global.CancellationTokenSource.Dispose();
            }
            try
            {
                L.Error(e.ExceptionObject as Exception, "An unhandled runtime exception occurred. OLAF CLI will terminate.");
                Global.Logger.Close();
            }
            catch (Exception exc)
            {
                Console.WriteLine("An unhandled runtime exception occurred. Additionally an exception was thrown logging this event: {0}\n{1}\n OLAF CLI will terminate.", exc.Message, exc.StackTrace);
            }
            if (e.IsTerminating)
            {
                Environment.Exit((int) ExitCode.UnhandledException);
            }
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            L.Info("Stop requested by user.");
            Exit(ExitCode.Successsd);
        }

        static void Exit(ExitCode result)
        {
            L.Close();
            Environment.Exit((int)result);
        }
    }
}
