using System;
using System.Diagnostics;
using System.Drawing;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
using System.Threading.Tasks;

namespace OLAF.Services.Classifiers
{
    public class ViolaJonesFaceDetector : Service<ArtifactMessage, Message>
    {
        #region Constructors
        public ViolaJonesFaceDetector(Profile profile, params Type[] clients) : base(profile, clients)
        {
            Cascade = new FaceHaarCascade();
            Detector = new HaarObjectDetector(Cascade, 30);
            Status = ApiStatus.Initializing;
        }
        #endregion

        #region Overridden members
        public override ApiResult Init()
        {
            Detector.SearchMode = ObjectDetectorSearchMode.NoOverlap;
            Detector.ScalingMode = ObjectDetectorScalingMode.SmallerToGreater;
            Detector.ScalingFactor = 1.5f;
            Detector.UseParallelProcessing = true;
            Detector.Suppression = 2;
            Status = ApiStatus.Initialized;
            Info("Accord.NET Viola-Jones face detector service initialized.");
            return ApiResult.Success;
        }

        protected override ApiResult ProcessClientQueue(ArtifactMessage message)
        {
            using (var op = Begin("Viola-Jones face detection"))
            {
                Image = Accord.Imaging.Image.FromFile(message.ArtifactPath);
                Rectangle[] objects = Detector.ProcessFrame(Image);
                op.Complete();
            }
            return ApiResult.Success;
        }
        #endregion

        #region Properties
        HaarCascade Cascade { get; }
        HaarObjectDetector Detector { get; }
        Bitmap Image { get; set; }
        #endregion'
    }
}
