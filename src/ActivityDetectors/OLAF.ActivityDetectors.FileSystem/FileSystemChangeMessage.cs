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
        public FileSystemChangeMessage(string path, WatcherChangeTypes type) : base(null, null)
        {
            Path = path;
            ChangeTypes = type;
        }
        #endregion

        #region Properties
        public string Path { get; }
        public WatcherChangeTypes ChangeTypes { get; }
        #endregion

    }
}
