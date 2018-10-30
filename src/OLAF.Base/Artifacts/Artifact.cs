using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class Artifact : Message
    {
        #region Constructors
        public Artifact(long id) : base(id)
        {
            
        }
        #endregion

        #region Properties
        #endregion
    }
}
