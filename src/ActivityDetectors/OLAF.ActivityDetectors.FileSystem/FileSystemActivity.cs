using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace OLAF.ActivityDetectors
{
    public class FileSystemActivity : ActivityDetector<FileSystemChangeMessage>
    {
        #region Constructors
        public FileSystemActivity(string path, string filter, Type mt) : base(mt)
        {
            FileSystemWatcher = new FileSystemWatcher(path, filter);
            FileSystemWatcher.Created += FileSystemActivity_Created;
            FileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.Size;
            Path = path;
            Status = ApiStatus.Ok;
        }
        #endregion

        #region Overriden members
        public override ApiResult Enable()
        {
            if (Status != ApiStatus.Ok)
            {
                throw new InvalidOperationException("An error ocurred during detector creation.");
            }
            FileSystemWatcher.EnableRaisingEvents = true;
            return FileSystemWatcher.EnableRaisingEvents ? ApiResult.Success : ApiResult.Failure;
        }
        #endregion

        #region Properties
        public string Path { get; protected set; }

        protected FileSystemWatcher FileSystemWatcher { get; set; }
        #endregion

        #region Event Handlers
        private void FileSystemActivity_Created(object sender, FileSystemEventArgs e)
        {
            Global.MessageQueue.Enqueue<FileSystemActivity>(new FileSystemChangeMessage(Interlocked.Increment(ref messageId), e.FullPath, e.ChangeType));
        }
        #endregion
    }
}
