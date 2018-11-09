using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OLAF
{
    public abstract class Message
    {
        #region Constructors
        protected Message(long id)
        {
            Id = id;
        }
        
        protected Message()
        {
            Id = Interlocked.Increment(ref rollingMessageId);
        }
        #endregion

        #region Properties
        public long Id;
        public static long rollingMessageId = 0;
        #endregion
    }
}
