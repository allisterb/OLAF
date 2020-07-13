using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OLAF
{
    public abstract class ActivityDetector<TMessage>: OLAFApi<ActivityDetector<TMessage>, TMessage>, IActivityDetector
        where TMessage : Message
    {
        #region Constructors
        public ActivityDetector(int pid, IMonitor monitor, Type mt)
        {
            processId = pid;
            Monitor = monitor;
            monitorType = mt;
        }

        public ActivityDetector(IMonitor monitor, Type mt) : this(0, monitor, mt) {}
        #endregion

        #region Abstract methods
        public abstract ApiResult Enable();
        #endregion

        #region Properties
        public IMonitor Monitor { get; }
        public long CurrentArtifactId => currentMessageId;
        #endregion

        #region Fields
        protected Type monitorType;
        protected int processId;
        #endregion
    }
}
