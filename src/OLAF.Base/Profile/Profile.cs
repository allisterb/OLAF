using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class Profile
    {
        #region Properties
        public List<Monitor> Monitors { get; }
        #endregion
    }
}
