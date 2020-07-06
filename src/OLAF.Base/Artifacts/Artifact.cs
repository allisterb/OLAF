using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class Artifact : Message
    {
        #region Constructors
        public Artifact() 
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                CurrentWindowTitle = Win32.Interop.GetCurrentWindowTitle();
            }
            MachineName = Environment.MachineName;
            UserName = Environment.UserName;
        }

        public Artifact(long id ) : base(id)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                CurrentWindowTitle = Win32.Interop.GetCurrentWindowTitle();
            }
            MachineName = Environment.MachineName;
            UserName = Environment.UserName;
        }
        #endregion

        #region Properties
        protected static ILogger L => Global.Logger;

        public virtual string Name { get; protected set; }

        public bool Preserve { get; set; }

        public Artifact Source { get; set; }

        public DateTime CreationTime { get; } = DateTime.Now;

        public string CurrentProcess { get; set; }

        public string CurrentWindowTitle { get; set; } 

        public string MachineName { get; set; }

        public string UserName { get;  }

        public bool HasFileSource => (Source != null) && Source.GetType() == typeof(FileArtifact);
        #endregion

        #region Methods
        [DebuggerStepThrough]
        protected static void Info(string messageTemplate, params object[] propertyValues) =>
           L.Info(messageTemplate, propertyValues);

        [DebuggerStepThrough]
        protected static void Debug(string messageTemplate, params object[] propertyValues) =>
            L.Debug(messageTemplate, propertyValues);

        [DebuggerStepThrough]
        protected static void Warn(string messageTemplate, params object[] propertyValues) =>
            L.Warn(messageTemplate, propertyValues);

        [DebuggerStepThrough]
        protected static void Error(string messageTemplate, params object[] propertyValues) =>
            L.Error(messageTemplate, propertyValues);

        [DebuggerStepThrough]
        protected static void Error(Exception e, string messageTemplate, params object[] propertyValues) =>
            L.Error(e, messageTemplate, propertyValues);

        [DebuggerStepThrough]
        protected static void Verbose(string messageTemplate, params object[] propertyValues) =>
            L.Verbose(messageTemplate, propertyValues);

        [DebuggerStepThrough]
        protected static IOperationContext Begin(string messageTemplate, params object[] propertyValues) =>
            L.Begin(messageTemplate, propertyValues);
        #endregion
    }
}
