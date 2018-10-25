using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OLAF.Service
{
    public abstract class Service<TMessage> : OLAFApi<Service<TMessage>> where TMessage : Message
    {
        #region Constructors
        public Service(Profile profile) : base()
        {
            Profile = profile;
        }
        #endregion

        #region Properties
        public Profile Profile { get; }

        public bool ShutdownRequested => shutdownRequested;

        public bool ShutdownCompleted => shutdownCompleted;

        protected List<Thread> Threads { get; set; }
        #endregion

        #region Fields
        protected bool shutdownRequested = false;
        protected bool shutdownCompleted = false;
        #endregion
    }
}
