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

            if (!GetDataDirectorySubDirExists("artifacts"))
            {
                BaseArtifactsDirectory = DataDirectory.CreateSubdirectory("artifacts");
            }
            else BaseArtifactsDirectory = new DirectoryInfo(GetDataDirectoryPathTo("artifacts"));


            if (!GetDataDirectorySubDirExists("dictionaries"))
            {
                DictionariesDirectory = DataDirectory.CreateSubdirectory("dictionaries");
            }
            else DictionariesDirectory = new DirectoryInfo(GetDataDirectoryPathTo("dictionaries"));
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

        public virtual string Name => type.Name;

        public string Description
        {
            get
            {
                Attribute a = Attribute.GetCustomAttribute(Type, typeof(DescriptionAttribute));
                if (a != null)
                {
                    return (a as DescriptionAttribute).Description;
                }
                else
                {
                    return string.Empty;
                }

            }
        }

        public ApiStatus Status { get; protected set; } = ApiStatus.Unknown;

        public static DirectoryInfo AssemblyDirectory { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

        public static Version AssemblyVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version;

        public static DirectoryInfo CurrentDirectory { get; } = new DirectoryInfo(Directory.GetCurrentDirectory());

        public static DirectoryInfo DataDirectory { get; }

        public static DirectoryInfo LogDirectory { get; }

        public static DirectoryInfo BaseArtifactsDirectory { get; }

        public static DirectoryInfo DictionariesDirectory { get; }

        protected static ILogger L => Global.Logger;
        #endregion

        #region Methods
        [DebuggerStepThrough]
        public static string GetAssemblyDirectoryPathTo(string path) =>
            Path.Combine(AssemblyDirectory.FullName, path);

        [DebuggerStepThrough]
        public static string GetCurrentDirectoryPathTo(params string[] paths) =>
            Path.Combine(CurrentDirectory.FullName, Path.Combine(paths));

        [DebuggerStepThrough]
        public static string GetLogDirectoryPathTo(params string[] paths) =>
            Path.Combine(LogDirectory.FullName, Path.Combine(paths));

        [DebuggerStepThrough]
        public static string GetDataDirectoryPathTo(params string[] paths) =>
            Path.Combine(DataDirectory.FullName, Path.Combine(paths));

        [DebuggerStepThrough]
        public static bool GetDataDirectoryFileExists(params string[] paths) =>
           File.Exists(GetDataDirectoryPathTo(paths));

        [DebuggerStepThrough]
        public static bool GetDataDirectorySubDirExists(params string[] paths) =>
           Directory.Exists(GetDataDirectoryPathTo(paths));

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
            Info("{0} initialized.", Type.Name);
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
        protected ApiResult SetErrorStatusAndReturnFailure(string errorMessage = "")
        {
            if (errorMessage.IsNotEmpty())
            {
                Error(errorMessage);
            }
            Status = ApiStatus.Error; 
            return ApiResult.Failure;
        }
        #endregion

        #region Fields
        protected Type type;
        protected CancellationToken cancellationToken;
        protected static long currentMessageId;
        #endregion
    }
}
