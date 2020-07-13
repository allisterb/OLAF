using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace OLAF.ActivityDetectors
{
    public class FileSystemActivity : ActivityDetector<FileSystemChangeMessage>, IDisposable
    {
        #region Constructors
        public FileSystemActivity(string path, string filter, bool includeSubDirs, IMonitor monitor, Type mt) : base(monitor, mt)
        {
            FileSystemWatcher = new FileSystemWatcher(path, filter);
            FileSystemWatcher.IncludeSubdirectories = includeSubDirs;
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

        public bool IsDisposed { get; protected set; } = false;
        protected FileSystemWatcher FileSystemWatcher { get; set; }
        #endregion

        #region Disposer and Finalizer
        /// /// </remarks>         
        public void Dispose()
        {
            Dispose(true); 
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time. 
            // Always use SuppressFinalize() in case a subclass 
            // of this type implements a finalizer. GC.SuppressFinalize(this); }
        }

        protected void Dispose(bool isDisposing)
        {
            try
            {
                if (!this.IsDisposed)
                {
                    // Explicitly set root references to null to expressly tell the GarbageCollector 
                    // that the resources have been disposed of and its ok to release the memory 
                    // allocated for them.

                    if (isDisposing)
                    {
                        // Release all managed resources here 
                        // Need to unregister/detach yourself from the events. Always make sure 
                        // the object is not null first before trying to unregister/detach them! 
                        // Failure to unregister can be a BIG source of memory leaks 
                        //if (someDisposableObjectWithAnEventHandler != null)
                        //{ someDisposableObjectWithAnEventHandler.SomeEvent -= someDelegate; 
                        //someDisposableObjectWithAnEventHandler.Dispose(); 
                        //someDisposableObjectWithAnEventHandler = null; } 
                        // If this is a WinForm/UI control, uncomment this code 
                        //if (components != null) //{ // components.Dispose(); //} } 
                        if (!ReferenceEquals(this.FileSystemWatcher, null))
                        {
                            this.FileSystemWatcher.Created -= FileSystemActivity_Created;
                            this.FileSystemWatcher.Dispose();
                            this.FileSystemWatcher = null;
                        }
                    }
                    // Release all unmanaged resources here 
                    // (example) if (someComObject != null && Marshal.IsComObject(someComObject)) { Marshal.FinalReleaseComObject(someComObject); someComObject = null; 
                }
            }
            catch (Exception e)
            {
                Error(e, "Exception thrown during disposal of Ssh audit environment.");
            }
            finally
            {
                this.IsDisposed = true;
            }
            
        }

        ~FileSystemActivity()
        {
            this.Dispose(false);
        }
        #endregion

        #region Event Handlers
        private void FileSystemActivity_Created(object sender, FileSystemEventArgs e)
        {
            EnqueueMessage(new FileSystemChangeMessage(e.FullPath, monitorType, e.ChangeType));
        }
        #endregion
    }
}