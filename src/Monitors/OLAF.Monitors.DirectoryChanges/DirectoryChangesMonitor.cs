using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using OLAF.ActivityDetectors;

namespace OLAF.Monitors
{
    public class DirectoryChangesMonitor : FileSystemMonitor<FileSystemActivity, FileSystemChangeMessage, FileArtifact>, IDisposable
    {
        #region Constructors
        public DirectoryChangesMonitor(string[] dirs, string[] exts, Profile profile) : base(dirs, exts, profile) {}
        #endregion

        #region Overridden members
        public override ApiResult Init()
        {
            if (Status != ApiStatus.Initializing) return ApiResult.Failure;
            try
            {
                
                for (int i = 0; i < Paths.Count; i++)
                {
                    KeyValuePair<DirectoryInfo, string> path = Paths.ElementAt(i);
                    Detectors.Add(new FileSystemActivity(path.Key.FullName, path.Value, true, this,
                        typeof(DirectoryChangesMonitor)));
                }
                return SetInitializedStatusAndReturnSucces();
            }
            catch (Exception e)
            {
                Error(e, "Error occurred initializing a detector.");
                return SetErrorStatusAndReturnFailure();
            }
        }

        public override ApiResult Shutdown()
        {
            if(base.Shutdown() != ApiResult.Success) return ApiResult.Failure;
            Debug("Disposing of {0} FileSystemWatchers.", Detectors.Count);
            foreach (FileSystemActivity fsa in Detectors)
            {
                fsa.Dispose();
            }
            return ApiResult.Success;
        }

        protected override ApiResult ProcessDetectorQueueMessage(FileSystemChangeMessage message)
        {
            if (!this.Paths.Any(p => message.Path.StartsWith(p.Key.FullName)))
            {
                return ApiResult.NoOp;
            }
            long aid = message.Id;
            string artifactName = string.Format("{0}_{1}", aid, Path.GetFileName(message.Path));
            string artifactPath = Profile.GetArtifactsDirectoryPathTo(artifactName);
            Debug("Waiting a bit for file to complete write...");
            Thread.Sleep(300);
            if (TryOpenFile(message.Path, out FileInfo f))
            {
                var hwnd = Win32.UnsafeNativeMethods.GetForegroundWindow();
                Win32.UnsafeNativeMethods.GetWindowThreadProcessId(hwnd, out var pid);
                var title = Win32.Interop.GetWindowTitle(hwnd);
                var process = Process.GetProcessById((int)pid);
                Info("Current process is {0} with window title {1}.", process.ProcessName, title);
                if (f.Length < 1024 * 1024 * 500)
                {
                    if (TryReadFile(message.Path, out byte[] data))
                    {
                        Debug("Read {0} bytes from {1}.", data.Length, message.Path);
                        File.WriteAllBytes(artifactPath, data);
                        Debug("Wrote {0} bytes to {1}.", data.Length, artifactPath);
                        var artifact = new FileArtifact(aid, artifactPath, message.Path, data);
                        artifact.CurrentProcess = process.ProcessName;
                        artifact.CurrentWindowTitle = title;
                        Global.MessageQueue.Enqueue<DirectoryChangesMonitor>(artifact);
                        return ApiResult.Success;
                    }
                    else
                    {
                        Error("Could not read artifact data from {0}.", message.Path);
                        return ApiResult.Failure;
                    }
                }
                else
                {
                    if (TryCopyFile(message.Path, artifactPath))
                    {
                        Debug("Copied artifact {0} to {1}.", message.Path, artifactPath);
                        var artifact = new FileArtifact(aid, artifactPath, message.Path);
                        artifact.CurrentProcess = process.ProcessName;
                        artifact.CurrentWindowTitle = title;
                        Global.MessageQueue.Enqueue<DirectoryChangesMonitor>(artifact);
                        return ApiResult.Success;
                    }
                    else
                    {
                        Error("Could not copy artifact {0} to {1}.", message.Path, artifactPath);
                        return ApiResult.Failure;
                    }
                }
            }
            else
            {
                Error("Could not open {0}.", message.Path);
                return ApiResult.Failure;
            }
            
        }

        protected bool TryCopyFile(string oldPath, string newPath, int maxTries = 100)
        {
            int tries = 0;
            while (tries < maxTries)
            {
                try
                {
                    File.Copy(oldPath, newPath, false);
                    return true;
                }
                catch (IOException)
                {
                    Debug("{0} file locked. Pausing a bit and then retrying copy ({1})...", oldPath, ++tries);
                    Thread.Sleep(50);
                }
                catch (Exception e)
                {
                    Error(e, "Unknown error attempting to copy {0}. Aborting.", oldPath);
                    return false;
                }
            }
            return false;
        }

        protected bool TryReadFile(string path, out byte[] data, int maxTries = 100)
        {
            int tries = 0;
            data = null;
            while (tries < maxTries)
            {
                try
                {
                    data = File.ReadAllBytes(path);
                    return true;
                }
                catch (IOException)
                {
                    Debug("{0} file locked. Pausing a bit and then retrying read ({1})...", path, ++tries);
                    Thread.Sleep(200);
                }
                catch (Exception e)
                {
                    data = null;
                    Error(e, "Unknown error attempting to read {0}. Aborting.", path);
                    return false;
                }
            }
            return false;
        }

        protected bool TryOpenFile(string path, out FileInfo file, int maxTries = 100)
        {
            file = null;
            int tries = 0;
            while (tries < maxTries)
            {
                try
                {
                    file = new FileInfo(path);
                    if (file.Exists)
                    {
                        return true;
                    }
                }
                catch (IOException)
                {
                    Debug("{0} file locked. Pausing a bit and then retrying open ({1})...", path, ++tries);
                    Thread.Sleep(50);
                }
                catch (Exception e)
                {
                    Error(e, "Unknown error attempting to read {0}. Aborting.", path);
                    return false;
                }
            }
            return false;
        }
        #endregion

        #region Properties
        public bool IsDisposed { get; protected set; }
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
                        foreach (var d in Detectors)
                        {
                            if (!ReferenceEquals(d, null))
                            {
                                d.Dispose();
                            }
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

        ~DirectoryChangesMonitor()
        {
            this.Dispose(false);
        }
        #endregion
    }
}
