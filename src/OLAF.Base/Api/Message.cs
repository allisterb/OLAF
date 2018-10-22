using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class Message
    {
        #region Constructors
        public Message(int? processId, int? threadId)
        {
            ProcessId = processId;
            ThreadId = threadId;
        }
        #endregion

        #region Properties
        public int? ProcessId { get; }

        public int? ThreadId { get; }
        #endregion
    }
}
