// RemoteFileMonitor (File: FileMonitorHook\ServerInterface.cs)
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
// Please visit https://easyhook.github.io for more information
// about the project, latest updates and other tutorials.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OLAF;
namespace OLAF.Hooks.Windows
{
    /// <summary>
    /// Provides an interface for communicating from the client (target) to the server (injector)
    /// </summary>
    public class EasyHookIpcServerInterface : MarshalByRefObject
    {
        /// <summary>
        /// Output the message to the console.
        /// </summary>
        /// <param name="fileNames"></param>
        public void ReportMessages(string[] messages)
        {
            for (int i = 0; i < messages.Length; i++)
            {
                Info(messages[i]);
            }
        }

        public void ReportMessage(string message)
        {
            Info(message);
        }

        /// <summary>
        /// Report exception
        /// </summary>
        /// <param name="e"></param>
        public void ReportException(Exception e)
        {
            Error(e, "");
            Console.WriteLine("The target process has reported an error:\r\n" + e.ToString());
        }

        //int count = 0;
        /// <summary>
        /// Called to confirm that the IPC channel is still open / host application has not closed
        /// </summary>
        public void Ping()
        {
           
        }

 
        protected static ILogger L => Global.Logger;

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

        protected static void SetPropFromDict(Type t, object o, Dictionary<string, object> p)
        {
            foreach (var prop in t.GetProperties())
            {
                if (p.ContainsKey(prop.Name) && prop.PropertyType == p[prop.Name].GetType())
                {
                    prop.SetValue(o, p[prop.Name]);
                }
            }
        }
    }
}
