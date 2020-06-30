using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Windows.Forms;
using System.Threading;

namespace OLAF.Win32
{
    //
    // Summary:
    //     Implements a Windows message.
    public struct WindowsMessage
    {
        //
        // Summary:
        //     Gets or sets the window handle of the message.
        //
        // Returns:
        //     The window handle of the message.
        public IntPtr HWnd { get; set; }
        //
        // Summary:
        //     Gets or sets the ID number for the message.
        //
        // Returns:
        //     The ID number for the message.
        public int Msg { get; set; }
        //
        // Summary:
        //     Gets or sets the System.Windows.Forms.Message.WParam field of the message.
        //
        // Returns:
        //     The System.Windows.Forms.Message.WParam field of the message.
        public IntPtr WParam { get; set; }
        //
        // Summary:
        //     Specifies the System.Windows.Forms.Message.LParam field of the message.
        //
        // Returns:
        //     The System.Windows.Forms.Message.LParam field of the message.
        public IntPtr LParam { get; set; }
        //
        // Summary:
        //     Specifies the value that is returned to Windows in response to handling the message.
        //
        // Returns:
        //     The return value of the message.
        public IntPtr Result { get; set; }

        public WindowsMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, IntPtr result)
        {
            HWnd = hwnd;
            Msg = msg;
            WParam = wparam;
            LParam = lparam;
            Result = result;
        }
    }

    public class MessagePump : NativeWindow
    {
        public event EventHandler<WindowsMessage> MessageReceived; 
        public bool IsStopped = false;
        public CancellationToken Token;
        public AutoResetEvent Sync; 

        public MessagePump(Dictionary<string, object> props)
        {
            EventHandler<WindowsMessage> handler = props.ContainsKey("handler") ? (EventHandler<WindowsMessage>)props["handler"] : null;
            if (handler != null)
            {
                MessageReceived += handler;
            }
            Token = (CancellationToken)props["cancellation_token"];
            Sync = (AutoResetEvent)props["sync"];
            CreateHandle(new CreateParams());
        }

        
        protected override void WndProc(ref Message msg)
        {
            if (Token.IsCancellationRequested)
            {
                Application.ExitThread();
                IsStopped = true;
            }
            else
            {
                MessageReceived?.Invoke(this, new WindowsMessage(msg.HWnd, msg.Msg, msg.WParam, msg.LParam, msg.Result));
            }
            // filter messages here for your purposes
            base.WndProc(ref msg);

        }

        public static void Run(object _props)
        {
            var props = (Dictionary<string, object>)_props;
            var p = new MessagePump(props);
            p.Sync.Set();
            Console.WriteLine("Handle is {0}.", p.Handle);
            Application.Run();
        }


    }

    /**
    public class MessagePump
    {
        private readonly Thread messagePump;
        private AutoResetEvent messagePumpRunning = new AutoResetEvent(false);

        public MessagePump(CancellationToken token, MessageHandler handler)
        {
            // start message pump in its own thread
            messagePump = new Thread(RunMessagePump { Name = "ManualMessagePump" };
            messagePump.Start();
            messagePumpRunning.WaitOne();
        }

        // Message Pump Thread
        private void RunMessagePump()
        {
            // Create control to handle windows messages
            MessageHandler messageHandler = new MessageHandler(token);
            messagePumpRunning.Set();
            Application.Run();
        }
    }
    */
}