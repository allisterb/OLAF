using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.ActivityDetectors
{
    [Serializable]
    public class FileSystemChangeMessage : Message
    {
        #region Constructors
        public FileSystemChangeMessage(string path, Type mt, WatcherChangeTypes type) : base()
        {
            Path = path;
            MonitorType = mt;
            ChangeTypes = type;
        }
        #endregion

        #region Properties
        public string Path { get; }
        public Type MonitorType { get; }
        public WatcherChangeTypes ChangeTypes { get; }
        #endregion
    }
}
