using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class ClassifierMessage : Message
    {
        #region Constructors
        public ClassifierMessage(ArtifactMessage artifact) : base(artifact.Id)
        {
            Artifact = artifact;
        }
        #endregion

        #region Properties
        public ArtifactMessage Artifact;
        #endregion
    }
}
