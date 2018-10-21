using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class OLAFApi<T>
    {
        #region Cosnstructors
        public OLAFApi()
        {
            if (L == null)
            {
                throw new InvalidOperationException("A logger is not assigned.");
            }
            cancellationToken = Global.CancellationTokenSource.Token;
        }
        #endregion

        #region Properties
        public ApiStatus Status { get; protected set; } = ApiStatus.Unknown;

        protected static DirectoryInfo AssemblyDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

        protected static Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

        protected static ILogger L => Global.Logger;
        #endregion

        #region Methods
        protected static string GetAssemblyDirectoryFullPath(string path) =>
            Path.Combine(AssemblyDirectory.FullName, path);

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

        protected static void SetPropFromDict(Type t, object o, Dictionary<string, object> p)
        {
            foreach (var prop in t.GetProperties())
            {
                if (p.ContainsKey(prop.Name) && prop.PropertyType == p[prop.Name].GetType())
                {
                    prop.SetValue(o, p[prop.Name]);
                }
            }
        }

        protected void SetPropFromDict(object o, Dictionary<string, object> p) => SetPropFromDict(typeof(T), o, p);
        #endregion

        #region Fields
        protected CancellationToken cancellationToken;
        #endregion
    }
}
