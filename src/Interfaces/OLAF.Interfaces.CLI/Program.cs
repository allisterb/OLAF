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
        public static List<Monitor> Monitors { get; protected set; }
       
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
            
            Monitor m = new ExplorerMonitor(
                GetCurrentProcesses().First(p => p.Value == "explorer").Key);
            if (m.Status != ApiStatus.Ok )
            {
                Error("Could not load monitor {0}.", typeof(ExplorerMonitor).Name);
                Exit(ExitCode.UnhandledException);
            }
            m.Init();
            if (m.Status != ApiStatus.Initialized)
            {
                Error("Could not initialize monitor {0}.", typeof(ExplorerMonitor).Name);
                Exit(ExitCode.UnhandledException);
            }
            m.Start();
        }

        static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception cancelException = null;
            try
            {
                if (Global.Logger != null && Global.CancellationTokenSource != null)
                {
                    if (!Global.CancellationTokenSource.IsCancellationRequested)
                    {
                        Global.CancellationTokenSource.Cancel();
                    }
                    Global.CancellationTokenSource.Dispose();
                }
            }
            catch(Exception ee)
            {
                cancelException = ee;
            }

            if (Global.Logger != null)
            {
                try

                {
                    Error(e.ExceptionObject as Exception, "An unhandled runtime exception occurred. OLAF CLI will terminate.");
                    if (cancelException != null)
                    {
                        Error(cancelException, "Additionally an exception was thrown attempting to stop all running threads.");
                    }
                    Global.Logger.Close();
                }
                catch (Exception exc)
                {
                    Console.WriteLine("An unhandled runtime exception occurred. Additionally an exception was thrown logging this event: {0}\n{1}\n OLAF CLI will terminate.", 
                        exc.Message, exc.StackTrace);
                }
            }
            else
            {
                Console.WriteLine("An unhandled runtime exception occurred. OLAF CLI will terminate.");
                if (e.ExceptionObject is Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
                if (cancelException != null)
                {
                    Console.WriteLine("Additionally an exception was thrown attempting to stop all running threads.");
                    Console.WriteLine(cancelException.Message);
                }
            }

            if (e.IsTerminating)
            {
                Environment.Exit((int) ExitCode.UnhandledException);
            }
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Info("Stopping...");
            if (Global.CancellationTokenSource.IsCancellationRequested)
            {
                Global.CancellationTokenSource.Cancel();
            }
            
            Exit(ExitCode.Success);
        }

        static void Exit(ExitCode result)
        {
            L.Close();
            Environment.Exit((int)result);
        }
    }
}
