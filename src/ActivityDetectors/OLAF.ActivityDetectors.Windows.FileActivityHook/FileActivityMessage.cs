using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.ActivityDetectors.Windows
{
    [Serializable]
    public class FileActivityMessage : Message
    {
        #region Constructors
        public FileActivityMessage(long id, int processId, int threadId, FileOp op, string path) : base(id)
        {
            ProcessId = processId;
            ThreadId = threadId;
            Op = op;
            Path = path;
        }
        #endregion

        #region Properties
        public int ProcessId { get; }

        public int ThreadId { get; }

        public FileOp Op { get; }

        public string Path { get; }
        #endregion

    }
}
