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

        public bool HasBitmap => Image != null;

        public Dictionary<ImageObjectKinds, List<Rectangle>> DetectedObjects { get; }
            = new Dictionary<ImageObjectKinds, List<Rectangle>>();

        //public Dictionary<str>
        public List<ArtifactCategory> Catgegories { get; } = new List<ArtifactCategory>();

        public bool IsAdultContent { get; set; }

        public double AdultContentScore { get; set; }

        public bool IsRacy { get; set; }

        public double RacyContentScore { get; set; }
        #endregion

        #region Methods
        public bool HasDetectedObjects(ImageObjectKinds kind) => this.DetectedObjects.ContainsKey(kind)
            && this.DetectedObjects[kind].Count > 0;

        public void AddDetectedObject(ImageObjectKinds kind, Rectangle r)
        {
            if(!DetectedObjects.ContainsKey(kind))
                    {
                DetectedObjects.Add(ImageObjectKinds.FaceCandidate, new List<Rectangle>() { r });
            }
            else
            {
                DetectedObjects[kind].Add(r);
            }
        }
        #endregion
    }
}
