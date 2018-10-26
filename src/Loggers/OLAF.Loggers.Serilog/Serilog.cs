using System;
using System.Collections.Generic;

using Serilog;
using Serilog.Sinks;
using SerilogTimings;
using SerilogTimings.Extensions;

namespace OLAF.Loggers
{
    public class SerilogLogger : ILogger
    {
        #region Constructors
        internal SerilogLogger()
        {
            if (!LoggerConfigured)
            {
                throw new InvalidOperationException("The Serilog logger is not configured.");
            }
            L = Log.Logger;
        }
        #endregion

        #region Properties
        public static LoggerConfiguration LoggerConfiguration { get; protected set; }

        public static bool LoggerConfigured { get; protected set; }

        protected Serilog.ILogger L { get; set; }
        #endregion

        #region Methods
        public static SerilogLogger CreateDefaultLogger(string logFilename = "OLAF.log")
        {
            if (!LoggerConfigured)
            {       
                LoggerConfiguration = new LoggerConfiguration()
                    .WriteTo.RollingFile(logFilename,
                        outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message}{NewLine}{Exception}");
                Log.Logger = LoggerConfiguration.CreateLogger();
                LoggerConfigured = true;
            }
            return new SerilogLogger();
        }

        public static SerilogLogger CreateLogger(List<string> enabledOptions)
        {
            if (!LoggerConfigured)
            {
                LoggerConfiguration = new LoggerConfiguration()
                    .Enrich.WithThreadId();

                if (enabledOptions.Contains("WithLogFile"))
                {
                    LoggerConfiguration = LoggerConfiguration
                    .WriteTo.RollingFile("OLAF.log",
                        outputTemplate: "{Timestamp:HH:mm:ss}<{ThreadId:d2}> [{Level:u3}] {Message}{NewLine}{Exception}");
                }

                if (enabledOptions.Contains("WithDebugOutput"))
                {
                    LoggerConfiguration = LoggerConfiguration.MinimumLevel.Debug();
                }

                if (!enabledOptions.Contains("WithoutConsole"))
                {
                    LoggerConfiguration = LoggerConfiguration
                        .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss}<{ThreadId:d2}> [{Level:u3}] {Message}{NewLine}{Exception}");
                }
                Log.Logger = LoggerConfiguration.CreateLogger();
                LoggerConfigured = true;
                if (enabledOptions.Contains("WithDebugOutput"))
                {
                    Log.Logger.Information("Log level is {0}.", "Debug");
                }
                if (enabledOptions.Contains("WithLogFile"))
                {
                   Log.Logger.Information("Log file is OLAF-{0:D4}{1:D2}{2:D2}.log", 
                       DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
                }
                if (enabledOptions.Contains("WithoutConsole"))
                {
                    Log.Logger.Information("Not logging to console.");
                }
            }
            return new SerilogLogger();
        }
        public void Info(string messageTemplate, params object[] propertyValues) 
            => L.Information(messageTemplate, propertyValues);

        public void Debug(string messageTemplate, params object[] propertyValues)
            => L.Debug(messageTemplate, propertyValues);

        public void Error(string messageTemplate, params object[] propertyValues)
            => L.Error(messageTemplate, propertyValues);

        public void Error(Exception e, string messageTemplate, params object[] propertyValues)
            => L.Error(e, messageTemplate, propertyValues);

        public void Verbose(string messageTemplate, params object[] propertyValues)
            => L.Verbose(messageTemplate, propertyValues);

        public void Warn(string messageTemplate, params object[] propertyValues)
            => L.Warning(messageTemplate, propertyValues);

        public void Close() => Log.CloseAndFlush();

        public IOperationContext Begin(string messageTemplate, params object[] args)
        {
            Info(messageTemplate + " starting...", args);
            return new SerilogOperation(L.BeginOperation(messageTemplate, args));
        }
        #endregion
    }
}
