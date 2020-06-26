using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using CO = Colorful.Console;
using Figgle;
using OLAF.Loggers;
using OLAF.Profiles;

namespace OLAF
{
    class Program : Interface
    {
        #region Constructors
        static Program()
        {
            AppDomain.CurrentDomain.UnhandledException += Program_UnhandledException;
            Console.CancelKeyPress += Console_CancelKeyPress;
        }
        #endregion

        #region Properties
        public static bool WithLogFile { get; set; } = false;
        public static bool WithoutConsole { get; set; } = false;
        public static bool WithDebugOutput { get; set; } = false;
        public static Profile Profile { get; protected set; }
        #endregion

        #region Methods
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

            if (args.Contains("--debug"))
            {
                WithDebugOutput = true;
                enabledLogOptions.Add("WithDebugOutput");
            }

            if (args.Contains("--without-console"))
            {
                WithoutConsole = true;
                enabledLogOptions.Add("WithoutConsole");
            }

            CO.Write(FiggleFonts.Rectangles.Render("O.L.A.F"));
            CO.WriteLine("v{0}", AssemblyVersion.ToString(3));

            Global.SetupLogger(() => SerilogLogger.CreateLogger(enabledLogOptions));

            Global.SetupMessageQueue();

            //Profile = new UserBrowserActivity();
            Profile = new UserDownloads();
            if (Profile.Init() != ApiResult.Success)
            {
                Error("Could not initialize profile {0}.", Profile.Name);
                Exit(ExitCode.InitError);
            }
            
            if (Profile.Start() != ApiResult.Success)
            {
                Error("Could not start profile {0}.", Profile.Name);
                Exit(ExitCode.StartError);
            }
            Info("Profile {0} started. Press any key to exit.", Profile.Name);

            ConsoleKeyInfo key = Console.ReadKey();

            var s = Profile.Shutdown();
            Exit(s == ApiResult.Success ? ExitCode.Success : ExitCode.ShutdownError);
        }

        static void Exit(ExitCode result)
        {
            L.Close();
            Environment.Exit((int)result);
        }
        #endregion

        #region Event Handlers
        static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception cancelException = null;
            Exception stopException = null;
            Exception abortException = null;

            try
            {
                if (Global.CancellationTokenSource != null && !Global.CancellationTokenSource.IsCancellationRequested)
                { 
                    Global.CancellationTokenSource.Cancel();  
                }

            }
            catch(Exception ce)
            {
                cancelException = ce;
            }

            try
            {
                if (Profile != null)
                {
                    if (Profile.Monitors != null)
                    {
                        foreach (IMonitor monitor in Profile.Monitors)
                        {
                            if (monitor.Status == ApiStatus.Ok && !monitor.ShutdownCompleted)
                            {
                                monitor.Shutdown();
                            }
                        }
                    }
                    if (Profile.Pipeline != null && Profile.Pipeline.Status == ApiStatus.Ok)
                    {
                        Profile.Pipeline.Shutdown();
                    }
                }
            }
            catch (Exception se)
            {
                stopException = se;
            }

            try
            {
                if (Profile != null)
                {
                    if (Profile.Monitors != null)
                    {
                        foreach (Thread t in Profile.Monitors.Where(m => m.Threads != null)
                            .SelectMany(m => m.Threads))
                        {
                            if (t.IsAlive)
                            {
                                t.Abort();
                            }
                        }
                    }
                    if (Profile.Pipeline != null && Profile.Pipeline.Services != null)
                    {
                        foreach (Thread t in Profile.Pipeline.Services.Values
                            .Where(s => s.Threads != null)
                            .SelectMany(s => s.Threads))
                        {
                            if (t.IsAlive)
                            {
                                t.Abort();
                            }
                        }
                    }
                }
            }
            catch (Exception ae)
            {
                abortException = ae;
            }

            try
            {
                Error(e.ExceptionObject as Exception, "An unhandled runtime exception occurred. OLAF will shutdown.");
                if (cancelException != null)
                {
                    Error(cancelException, "Additionally an exception was thrown attempting to cancel all running service and monitors.");
                }

                if (stopException != null)
                {
                    Error(stopException, "Additionally an exception was thrown attempting to stop all running monitors and services.");
                }

                if (abortException != null)
                {
                    Error(abortException, "Additionally an exception was thrown attempting to abort all running threads.");
                }

                Global.Logger.Close();
            }

            catch (Exception logException)
            {
                Console.WriteLine("An unhandled runtime exception occurred. Additionally an exception was thrown logging this event: {0}\n{1}\n OLAF CLI will terminate.", 
                    logException.Message, logException.StackTrace);

                if (cancelException != null)
                {
                    Console.WriteLine("Additionally an exception was thrown attempting to cancel all running service and monitors: {0}.", cancelException);
                }

                if (stopException != null)
                {
                    Console.WriteLine("Additionally an exception was thrown attempting to stop all running service and monitors: {0}.", stopException);
                }

                if (abortException != null)
                {
                    Console.WriteLine("Additionally an exception was thrown attempting to abort all running threads: {0}.", abortException);
                }
            }
            Environment.Exit((int)ExitCode.UnhandledException);
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Info("Cancel key pressed. Stopping...");
            Exception cancelException = null;
            Exception stopException = null;
            Exception abortException = null;

            try
            {
                if (Global.CancellationTokenSource != null && !Global.CancellationTokenSource.IsCancellationRequested)
                {
                    Global.CancellationTokenSource.Cancel();
                }

            }
            catch (Exception ce)
            {
                cancelException = ce;
            }

            try
            {
                if (Profile != null)
                {
                    if (Profile.Monitors != null)
                    {
                        foreach (IMonitor monitor in Profile.Monitors)
                        {
                            if (monitor.Status == ApiStatus.Ok && !monitor.ShutdownCompleted)
                            {
                                monitor.Shutdown();
                            }
                        }
                    }
                    if (Profile.Pipeline != null && Profile.Pipeline.Status == ApiStatus.Ok)
                    {
                        Profile.Pipeline.Shutdown();
                    }
                }
            }
            catch (Exception se)
            {
                stopException = se;
            }

            try
            {
                if (Profile != null)
                {
                    if (Profile.Monitors != null)
                    {
                        foreach (Thread t in Profile.Monitors.SelectMany(m => m.Threads))
                        {
                            if (t.IsAlive)
                            {
                                t.Abort();
                            }
                        }
                    }
                    if (Profile.Pipeline != null && Profile.Pipeline.Services != null)
                    {
                        foreach (Thread t in Profile.Pipeline.Services.Values.SelectMany(s => s.Threads))
                        {
                            if (t.IsAlive)
                            {
                                t.Abort();
                            }
                        }
                    }
                }
            }
            catch (Exception ae)
            {
                abortException = ae;
            }

            if (cancelException != null)
            {
                Error(cancelException, "An exception was thrown attempting to cancel all running service and monitors.");
            }

            if (stopException != null)
            {
                Error(stopException, "An exception was thrown attempting to stop all running monitors and services.");
            }

            if (abortException != null)
            {
                Error(abortException, "An exception was thrown attempting to abort all running threads.");
            }

            Global.Logger.Close();
            Exit(ExitCode.CtrlC);
        }
        #endregion
    }
}
