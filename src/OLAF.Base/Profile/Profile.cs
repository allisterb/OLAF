using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class Profile : OLAFApi<Profile, Message>
    {
        #region Constructors
        public Profile(string name)
        {
            Name = name;
            string artifactsdirName = GetCurrentDirectoryPathTo("data", "artifacts", GetArtifactsDirectoryName());
            if (!Directory.Exists(artifactsdirName))
            {
                ArtifactsDirectory = Directory.CreateDirectory(artifactsdirName);
            }
            else ArtifactsDirectory = new DirectoryInfo(artifactsdirName);
            Status = ApiStatus.Initializing;
        }
        #endregion

        #region Abstract members
        public string Name { get; }
        public abstract ApiResult Init();
        public abstract ApiResult Start();
        #endregion

        #region Properties
        public static string[] ImageWildcardExtensions = {"*.bmp", "*.dib", "*.rle", "*.jpg", "*.jpeg", "*.jpe", "*.jfif", "*.gif", "*.tif",
            "*.tiff", "*.png"};

        public List<IMonitor> Monitors { get; protected set; }
        
        public DirectoryInfo ArtifactsDirectory { get; }
        #endregion

        #region Methods
        public virtual ApiResult Shutdown()
        {
            if (Status != ApiStatus.Ok)
            {
                throw new InvalidOperationException("This monitor is not started.");
            }
            Parallel.ForEach(Monitors, m => m.Shutdown());
            if (Monitors.All(m => m.ShutdownCompleted))
            {
                Status = ApiStatus.Ok;
                return ApiResult.Success;
            }
            else
            {
                Error("One or more of the monitors did not shutdown.");
                Status = ApiStatus.Error;
                return ApiResult.Failure;
            }
        }

        [DebuggerStepThrough]
        public string GetArtifactsDirectoryPathTo(params string[] paths) =>
            Path.Combine(ArtifactsDirectory.FullName, Path.Combine(paths));

        [DebuggerStepThrough]
        protected string GetArtifactsDirectoryName() =>
            string.Format("{0:D4}{1:D2}{2:D2}_{3}_{4}", DateTime.Today.Year, DateTime.Today.Month,
                DateTime.Today.Day, Name, DateTime.UtcNow.Ticks);

        #endregion
    }
}
