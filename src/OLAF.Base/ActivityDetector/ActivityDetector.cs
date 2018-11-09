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
        public ActivityDetector(int pid, Type mt)
        {
            processId = pid;
            monitorType = mt;
        }

        public ActivityDetector(Type mt)
        {
            processId = 0;
            monitorType = mt;
        }
        #endregion

        #region Abstract methods
        public abstract ApiResult Enable();
        #endregion

        #region Properties
        public long CurrentArtifactId => currentMessageId;
        #endregion

        #region Fields
        protected Type monitorType;
        protected int processId;
        #endregion
    }
}
