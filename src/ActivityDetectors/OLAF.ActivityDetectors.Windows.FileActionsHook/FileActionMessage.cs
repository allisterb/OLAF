using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.ActivityDetectors.Windows
{
    [Serializable]
    public class FileActionMessage : Message
    {
        #region Constructors
        public FileActionMessage(int processId, int threadId, FileOp op, string path) : base(processId, threadId)
        {
            Op = op;
            Path = path;
        }
        #endregion

        #region Properties
        public FileOp Op { get; }

        public string Path { get; }
        #endregion

    }
}
