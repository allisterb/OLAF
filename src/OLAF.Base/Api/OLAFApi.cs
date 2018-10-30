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
    public abstract class OLAFApi<TApi, TMessage>
        where TMessage : Message
    {
        #region Constructors
        static OLAFApi()
        {
            if (!Directory.Exists(GetCurrentDirectoryPathTo("data")))
            {
                DataDirectory = CurrentDirectory.CreateSubdirectory("data");
            }
            else
            {
                DataDirectory = new DirectoryInfo(GetCurrentDirectoryPathTo("data"));
            }

            if (!Directory.Exists(GetCurrentDirectoryPathTo("logs")))
            {
                LogDirectory = CurrentDirectory.CreateSubdirectory("logs");
            }
            else
            {
                LogDirectory = new DirectoryInfo(GetCurrentDirectoryPathTo("logs"));
            }

            if (!Directory.Exists(GetCurrentDirectoryPathTo("data", "artifacts")))
            {
                BaseArtifactsDirectory = DataDirectory.CreateSubdirectory("artifacts");
            }
            else BaseArtifactsDirectory = new DirectoryInfo(GetCurrentDirectoryPathTo("data", "artifacts"));

            if (!Directory.Exists(GetCurrentDirectoryPathTo("data", "dictionaries")))
            {
                BaseDictionariesDirectory = DataDirectory.CreateSubdirectory("dictionaries");
            }
            else BaseDictionariesDirectory = new DirectoryInfo(GetCurrentDirectoryPathTo("data", "dictionaries"));
        }

        public OLAFApi()
        {
            type = this.GetType();
            if (L == null)
            {
                throw new InvalidOperationException("A logger is not assigned.");
            }
            cancellationToken = Global.CancellationTokenSource.Token;
        }
        #endregion

        #region Properties
        public Type Type => type;

        public string Name => type.Name;

        public ApiStatus Status { get; protected set; } = ApiStatus.Unknown;

        protected static DirectoryInfo AssemblyDirectory { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

        protected static Version AssemblyVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version;

        protected static DirectoryInfo CurrentDirectory { get; } = new DirectoryInfo(Directory.GetCurrentDirectory());

        protected static DirectoryInfo DataDirectory { get; }

        protected static DirectoryInfo LogDirectory { get; }

        protected static DirectoryInfo BaseArtifactsDirectory { get; }

        protected static DirectoryInfo BaseDictionariesDirectory { get; }

        protected static ILogger L => Global.Logger;
        #endregion

        #region Methods
        [DebuggerStepThrough]
        protected static string GetAssemblyDirectoryPathTo(string path) =>
            Path.Combine(AssemblyDirectory.FullName, path);

        [DebuggerStepThrough]
        protected static string GetCurrentDirectoryPathTo(params string[] paths) =>
            Path.Combine(CurrentDirectory.FullName, Path.Combine(paths));

        [DebuggerStepThrough]
        protected static string GetLogDirectoryPathTo(params string[] paths) =>
            Path.Combine(LogDirectory.FullName, Path.Combine(paths));

        [DebuggerStepThrough]
        protected static string GetDataDirectoryPathTo(params string[] paths) =>
            Path.Combine(DataDirectory.FullName, Path.Combine(paths));

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

        protected void SetPropFromDict(object o, Dictionary<string, object> p) => SetPropFromDict(typeof(TApi), o, p);

        protected void ThrowIfNotInitializing()
        {
            if (Status != ApiStatus.Initializing) throw new Exception("Could not construct this object.");
        }

        [DebuggerStepThrough]
        protected virtual void EnqueueMessage(Message message) => Global.MessageQueue.Enqueue(type, message);

        [DebuggerStepThrough]
        protected void ThrowIfNotInitialized()
        {
            if (Status != ApiStatus.Initialized) throw new Exception("This object is not initialized.");
        }

        [DebuggerStepThrough]
        protected void ThrowIfNotOk()
        {
            if (Status != ApiStatus.Ok) throw new Exception("This object is not initialized.");
        }

        [DebuggerStepThrough]
        protected ApiResult SetStatusAndReturnSuccess(ApiStatus apiStatus)
        {
            Status = apiStatus;
            return ApiResult.Success;
        }

        [DebuggerStepThrough]
        protected ApiResult SetInitializedStatusAndReturnSucces()
        {
            Status = ApiStatus.Initialized;
            return ApiResult.Success;
        }

        [DebuggerStepThrough]
        protected ApiResult SetOkStatusAndReturnSucces()
        {
            Status = ApiStatus.Ok;
            return ApiResult.Success;
        }

        [DebuggerStepThrough]
        protected ApiResult SetStatusAndReturnFailure(ApiStatus apiStatus)
        {
            Status = apiStatus;
            return ApiResult.Failure;
        }

        [DebuggerStepThrough]
        protected ApiResult SetErrorStatusAndReturnFailure()
        {
            Status = ApiStatus.Error; ;
            return ApiResult.Failure;
        }
        #endregion

        #region Fields
        protected Type type;
        protected CancellationToken cancellationToken;
        #endregion
    }
}
