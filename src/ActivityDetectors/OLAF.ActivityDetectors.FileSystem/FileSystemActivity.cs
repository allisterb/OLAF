using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.ActivityDetectors
{
    public class FileSystemActivity : ActivityDetector
    {
        #region Constructors
        public FileSystemActivity(string path, string filter, Type mt) : base(mt)
        {
            FileSystemWatcher = new FileSystemWatcher(path, filter);
            FileSystemWatcher.Created += FileSystemActivity_Created;
            FileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.Size;
        }
        #endregion

        #region Properties
        protected FileSystemWatcher FileSystemWatcher { get; set; }
        #endregion

        #region Methods
        public ApiResult EnableEvents()
        {
            FileSystemWatcher.EnableRaisingEvents = true;
            return FileSystemWatcher.EnableRaisingEvents ? ApiResult.Success : ApiResult.Failure;
        }
        #endregion

        #region Event Handlers
        private void FileSystemActivity_Created(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
