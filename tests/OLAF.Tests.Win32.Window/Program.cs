using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using OLAF.Win32;
namespace OLAF.Tests.Win32.Window
{
    class Program
    {
        static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            AutoResetEvent messagePumpRunning = new AutoResetEvent(false);
            EventHandler<WindowsMessage> handler = (s, m) => Console.WriteLine(m.HWnd);  
            var props = new Dictionary<string, object>();
            props.Add("cancellation_token", cts.Token);
            props.Add("sync", messagePumpRunning);
            props.Add("handler", handler);
            var thread = new Thread(() => MessagePump.Run(props));
            thread.Start();
            messagePumpRunning.WaitOne();
            cts.Cancel();
            Console.WriteLine("{0}", thread.IsAlive);
            //Thread.Sleep(5000);
            //cts.Cancel();
            
        }
    }
}
