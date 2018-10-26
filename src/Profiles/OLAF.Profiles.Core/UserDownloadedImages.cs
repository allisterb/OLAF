using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OLAF.Monitors;
using OLAF.Pipelines;


namespace OLAF.Profiles
{
    public class UserDownloadedImages : Profile
    {
        #region Constructors
        public UserDownloadedImages()
        {
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
                return;
            }
            DirectoryChangesMonitor monitor = new DirectoryChangesMonitor(UserKnownFolders.ToArray(),
                ImageWildcardExtensions.ToArray(), this);
            if (monitor.Status != ApiStatus.Initializing)
            {
                Status = ApiStatus.Error;
            }
            Monitors.Add(monitor);
            Pipeline = new Pipeline1(this);
            Status = Pipeline.Status;
        }
        #endregion

        

        #region Properties
        public List<string> UserKnownFolders { get; protected set; }

        public Dictionary<string, string> Paths { get; protected set; }
        #endregion

        
    }
}
