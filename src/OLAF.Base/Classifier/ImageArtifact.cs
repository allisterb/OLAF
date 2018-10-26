using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public class ImageArtifact : ClassifierInput
    {
        #region Constructors
        public ImageArtifact(Artifact artifactMessage, Bitmap image) : base(artifactMessage)
        {
            Image = image;
        }
        #endregion

        #region Properties
        public Bitmap Image { get; }

        public Dictionary<ImageObjectKinds, List<Rectangle>> DetectedObjects { get; }
            = new Dictionary<ImageObjectKinds, List<Rectangle>>();
        #endregion

        #region Methods
        public bool HasDetectedObjects(ImageObjectKinds kind) => this.DetectedObjects.ContainsKey(kind)
            && this.DetectedObjects[kind].Count > 0;
        #endregion
    }
}
