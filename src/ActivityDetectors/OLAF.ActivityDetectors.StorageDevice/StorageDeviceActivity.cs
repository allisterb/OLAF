using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.ActivityDetectors
{
    public class StorageDeviceActivity : ActivityDetector<StorageDeviceActivityMessage>, IDisposable
    {
        #region Constructors
        public StorageDeviceActivity(Type mt) : base(mt) 
        {
            Watcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 or EventType = 3");
            Watcher.Query = query;
            Watcher.EventArrived += Watcher_EventArrived;
            Status = ApiStatus.Initialized;
        }
        #endregion

        #region Properties
        protected ManagementEventWatcher Watcher { get; set; }
        public bool IsDisposed { get; set; }
        #endregion

        #region Overriden members
        public override ApiResult Enable()
        {
            Watcher.Start();
            return ApiResult.Success;
        }

        #endregion

        #region Event handlers
        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            string driveName = e.NewEvent.Properties["DriveName"].Value.ToString();
            StorageActivityEventType eventType = ((Convert.ToInt16(e.NewEvent.Properties["EventType"].Value)) == 2) ? StorageActivityEventType.Inserted : StorageActivityEventType.Removed;
            EnqueueMessage(new StorageDeviceActivityMessage(eventType, driveName, DateTime.Now));
        }
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
                        if (!ReferenceEquals(this.Watcher, null))
                        {
                            this.Watcher.EventArrived -= Watcher_EventArrived;
                            this.Watcher.Dispose();
                            this.Watcher = null;
                        }
                    }
                    // Release all unmanaged resources here 
                    // (example) if (someComObject != null && Marshal.IsComObject(someComObject)) { Marshal.FinalReleaseComObject(someComObject); someComObject = null; 
                }
            }
            catch (Exception e)
            {
                Error(e, "Exception thrown during disposal of USB.");
            }
            finally
            {
                this.IsDisposed = true;
            }

        }

        ~StorageDeviceActivity()
        {
            this.Dispose(false);
        }
        #endregion
    }
}
