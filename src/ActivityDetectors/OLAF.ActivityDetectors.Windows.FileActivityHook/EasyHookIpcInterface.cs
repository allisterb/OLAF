#region Attribution and License
// Contains code from the EasyHook project

// RemoteFileMonitor (File: FileMonitorHook\InjectionEntryPoint.cs)
//
// Copyright (c) 2017 Justin Stenning
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// Please visit https://github.io for more information
// about the project, latest updates and other tutorials.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using OLAF;
namespace OLAF.ActivityDetectors.Windows
{
    /// <summary>
    /// Provides an interface for communicating from the client (target) to the server (injector)
    /// </summary>
    public class EasyHookIpcInterface : MarshalByRefObject
    {
        #region Constructors
        public EasyHookIpcInterface() {}
        #endregion

        #region Properties
        public bool IsCancellationRequested() => cancellationToken.IsCancellationRequested;


        public bool HookShutdownComplete { get; protected set; }

        protected static ILogger L => Global.Logger;

        protected CancellationToken cancellationToken = Global.CancellationTokenSource.Token;

        #endregion

        #region Methods
        /// <summary>
        /// Called to confirm that the IPC channel is still open / host application has not closed
        /// </summary>
        public void Ping()
        {
            Verbose("Ping from client-side IPC channel succeded.");
        }

        public void EnqueueFileActivity(int processId, int threadId, FileOp op, string path)
        {
            Global.MessageQueue.Enqueue<FileActivityHook>(new FileActivityMessage(processId, threadId, op, path));
        }

        public void EnqueueFileActivity(int processId, FileOp op, string path)
        {
            Global.MessageQueue.Enqueue<FileActivityHook>(new FileActivityMessage(processId, -1, op, path));
        }

        public void SetHookShutdownComplete() => HookShutdownComplete = true;

        [DebuggerStepThrough]
        public virtual void Info(string messageTemplate, params object[] propertyValues) =>
            L.Info(messageTemplate, propertyValues);

        [DebuggerStepThrough]
        public virtual void Debug(string messageTemplate, params object[] propertyValues) =>
            L.Debug(messageTemplate, propertyValues);

        [DebuggerStepThrough]
        public virtual void Error(string messageTemplate, params object[] propertyValues) =>
            L.Error(messageTemplate, propertyValues);

        [DebuggerStepThrough]
        public virtual void Error(Exception e, string messageTemplate, params object[] propertyValues) =>
            L.Error(e, messageTemplate, propertyValues);

        [DebuggerStepThrough]
        public virtual void Verbose(string messageTemplate, params object[] propertyValues) =>
            L.Verbose(messageTemplate, propertyValues);

        [DebuggerStepThrough]
        public virtual void Warn(string messageTemplate, params object[] propertyValues) =>
            L.Warn(messageTemplate, propertyValues);
        #endregion

        #region Fields
    
        #endregion
    }
}
