using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OLAF
{
    public class ArtifactMessage : Message
    {
        public ArtifactMessage(long id, string artifactPath) : base(id)
        {
            ArtifactPath = artifactPath;
        }

        #region Properties
        public string ArtifactPath { get; }
        public string ArtifactName => ArtifactPath?.GetPathFilename();
        #endregion
    }
}
