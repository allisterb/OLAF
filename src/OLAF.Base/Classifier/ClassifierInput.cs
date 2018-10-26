using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class ClassifierInput : Message
    {
        #region Constructors
        public ClassifierInput(Artifact artifact) : base(artifact.Id)
        {
            Artifact = artifact;
        }
        #endregion

        #region Properties
        public Artifact Artifact;
        #endregion
    }
}
