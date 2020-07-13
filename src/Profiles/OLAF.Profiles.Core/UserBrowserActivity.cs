using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OLAF.Monitors;
using OLAF.Monitors.Windows;
using OLAF.Pipelines;


namespace OLAF.Profiles
{
    public class UserBrowserActivity : Profile
    {
        #region Constructors
        public UserBrowserActivity()
        {
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

            Monitors.Add (new DirectoryChangesMonitor(UserKnownFolders.ToArray(),
                BasicImageWildcardExtensions.ToArray(), this));
            
            List<IMonitor> browserWindowMonitors = new List<IMonitor>()
            {
                new AppWindowMonitor(this, "chrome"),
                new AppWindowMonitor(this, "firefox"),
                new AppWindowMonitor(this, "MicrosoftEdgeCP"),
            };
            Monitors.AddRange(browserWindowMonitors);

            Pipeline = new ImagePipeline(this);
            Status = Pipeline.Status;
        }
        #endregion

        #region Properties
        public Dictionary<string, string> Paths { get; protected set; }
        #endregion

        
    }
}
