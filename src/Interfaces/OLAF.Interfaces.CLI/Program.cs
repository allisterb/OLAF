using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            CO.WriteLine(FiggleFonts.Rectangles.Render("OnLine Automated Forensics"));
            CO.WriteLine("v{0}", AssemblyVersion.ToString(3));
            Global.SetupLogger(() => SerilogLogger.CreateLogger(enabledLogOptions));

            Global.SetupMessageQueue();

             Profile = new UserDownloadedImages();

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
            try
            {
                if (Global.CancellationTokenSource != null && !Global.CancellationTokenSource.IsCancellationRequested)
                { 
                    Global.CancellationTokenSource.Cancel();  
                }
                if (Profile == null)
                {
                    if (Profile.Monitors != null)
                    {
                        foreach (IMonitor monitor in Profile.Monitors)
                        {
                            monitor.Shutdown();
                        }
                    }
                    if (Profile.Pipeline != null)
                    {
                        Profile.Pipeline.Shutdown();
                    }
                }
            }
            catch(Exception ee)
            {
                cancelException = ee;
            }

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
            
            if (e.IsTerminating)
            {
                Environment.Exit((int) ExitCode.UnhandledException);
            }
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (!Global.CancellationTokenSource.IsCancellationRequested)
            {
                Global.CancellationTokenSource.Cancel();
            }
            
            Exit(ExitCode.Success);
        }
        #endregion
    }
}
