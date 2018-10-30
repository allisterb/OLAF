using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OLAF.Monitors.Windows;
using OLAF.Pipelines;


namespace OLAF.Profiles
{
    public class Browsers : Profile
    {
        #region Constructors
        public Browsers()
        {
            AppWindowMonitor monitor = new AppWindowMonitor("chrome");
            if (monitor.Status != ApiStatus.Initializing)
            {
                Status = ApiStatus.Error;
            }
            Monitors.Add(monitor);
            Pipeline = new AppWindowPipeline(this);
            Status = Pipeline.Status;
        }
        #endregion

        #region Properties
        public List<string> UserKnownFolders { get; protected set; }

        public Dictionary<string, string> Paths { get; protected set; }
        #endregion

        
    }
}
