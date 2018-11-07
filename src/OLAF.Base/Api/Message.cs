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
        public Message(long id)
        {
            Id = id;
        }
        #endregion

        #region Properties
        public long Id;
        #endregion
    }
}
