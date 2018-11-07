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
        public Artifact(long id) : base(id)
        {
            
        }
        #endregion

        #region Properties
        protected static ILogger L => Global.Logger;

        public bool Preserve { get; set; }
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
