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
    public class DLP : Profile
    {
        public DLP()
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

            Monitors.Add(new DirectoryChangesMonitor(UserKnownFolders.ToArray(),
                DocumentWildcardExtensions, this));
            Monitors.Add(new StorageDeviceMonitor(BasicImageWildcardExtensions.ToArray(), this));
            Pipeline = new DocumentPipeline(this);
            Status = Pipeline.Status;
        }
    }
}
