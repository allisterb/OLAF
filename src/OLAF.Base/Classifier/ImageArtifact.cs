using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public class ImageArtifact : ClassifierMessage
    {
        #region Constructors
        public ImageArtifact(ArtifactMessage artifactMessage, Bitmap image) : base(artifactMessage)
        {
            Image = image;
        }
        #endregion

        #region Properties
        public Bitmap Image { get; }
        #endregion
    }
}
