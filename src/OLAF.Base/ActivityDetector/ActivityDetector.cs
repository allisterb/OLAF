using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OLAF
{
    public abstract class ActivityDetector : OLAFApi<ActivityDetector>
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

        #region Fields
        protected Type monitorType;
        protected int processId;
        #endregion
    }
}
