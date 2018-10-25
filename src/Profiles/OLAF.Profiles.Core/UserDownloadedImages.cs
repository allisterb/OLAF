using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OLAF.Monitors;

namespace OLAF.Profiles
{
    public class UserDownloadedImages : Profile
    {
        #region Constructors
        public UserDownloadedImages() : base("UserDownloadedImages") {}
        #endregion

        #region Properties
        public List<string> UserKnownFolders { get; protected set; }

        public Dictionary<string, string> Paths { get; protected set; }
        #endregion

        #region Overridden members
        public override ApiResult Init()
        {
            if (Status != ApiStatus.Initializing) return ApiResult.Failure;
            UserKnownFolders = new List<string>()
            {
                WindowsKnownFolders.GetPath(KnownFolder.Downloads),
                WindowsKnownFolders.GetPath(KnownFolder.Pictures),
                WindowsKnownFolders.GetPath(KnownFolder.Documents),
                WindowsKnownFolders.GetPath(KnownFolder.Desktop)
            };
            IEnumerable<string> missingFolders = UserKnownFolders.Where(f => !Directory.Exists(f));
            if (missingFolders.Count() > 0)
            {
                Warn("The following folders do not exist and will not be monitored: {0}", missingFolders);
                UserKnownFolders.RemoveAll(f => !Directory.Exists(f));
            }
            if (UserKnownFolders.Count == 0)
            {
                Error("No directories to monitor.");
                Status = ApiStatus.FileNotFound;
                return ApiResult.Failure;
            }
            
            DirectoryChangesMonitor monitor = new DirectoryChangesMonitor(UserKnownFolders.ToArray(), 
                ImageWildcardExtensions.ToArray(), this);
            if (monitor.Init() == ApiResult.Success)
            {
                Monitors = new List<IMonitor>(1) { monitor };
                Status = ApiStatus.Initialized;
                return ApiResult.Success;
            }
            else
            {
                Status = ApiStatus.Error;
                return ApiResult.Failure;
            }
        }

        public override ApiResult Start()
        {
            if (Status != ApiStatus.Initialized)
            {
                throw new InvalidOperationException("This monitor is not initialized");
            }
            if (Monitors.All(m => m.Start() == ApiResult.Success))
            {
                Status = ApiStatus.Ok;
                return ApiResult.Success;
            }
            else
            {
                Error("One or more of the monitors did not start.");
                Status = ApiStatus.Error;
                return ApiResult.Failure;
            }
        }
        #endregion
    }
}
