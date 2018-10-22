using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace OLAF
{
    public abstract class FileSystemMonitor<TDetector, TMessage> : Monitor
        where TDetector : ActivityDetector
        where TMessage : Message
    {
        #region Constructors
        public FileSystemMonitor(Dictionary<string, string> paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }
            else if (paths.Count == 0)
            {
                throw new ArgumentException("No paths specified.", nameof(paths));
            }

            Paths = new Dictionary<DirectoryInfo, string>(paths.Keys.Count);
            foreach (KeyValuePair<string, string> kv in paths)
            {
                try
                {
                    if (Directory.Exists(kv.Key))
                    {
                        DirectoryInfo dir = new DirectoryInfo(kv.Key);
                        string searchPattern = Path.Combine(dir.FullName, kv.Value);
                        //IEnumerable c = dir.EnumerateFileSystemInfos(kv.Value, SearchOption.AllDirectories);
                        var findFileData = new Native.WIN32_FIND_DATA();
                        IntPtr hFindFile = Native.FindFirstFile(searchPattern, ref findFileData);
                        if (hFindFile != Native.INVALID_HANDLE_VALUE)
                        {
                            Debug("Adding {0} {1} to monitored paths.", dir.FullName, kv.Value);
                            Paths.Add(dir, kv.Value);

                        }
                        else
                        {
                            Warn("Path {0} currently has no files.", kv.Value);
                            Debug("Adding {0} {1} to monitored paths.", dir.FullName, kv.Value);
                            Paths.Add(dir, kv.Value);
                        }
                    }
                    else
                    {
                        Warn("The directory {0} does not exist.", kv.Key);
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Error(e, "Error occurred enumerating files in directory {0} using search pattern {1}.",
                        kv.Key, kv.Value);
                    Debug("Not adding {0} to monitored paths.", kv.Key);
                    continue;
                }
            }
            if (Paths.Count > 0)
            {
                Status = ApiStatus.Ok;
            }
            else
            {
                Status = ApiStatus.FileNotFound;
            }
        }
        #endregion

        #region Properties
        protected Dictionary<DirectoryInfo, string> Paths;
        #endregion

        #region Methods
        protected override void MonitorQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    TMessage msg =
                        (TMessage) Global.MessageQueue.Dequeue<TMessage>(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    Info("Stopping {0} queue monitor.", typeof(TDetector).Name);
                    Status = ApiStatus.Ok;
                    return;
                }
                catch (Exception ex)
                {
                    Error(ex, "Exception thrown during {0} queue monitoring.", typeof(TDetector).Name);
                }
            }

        }
        #endregion
    }
}
