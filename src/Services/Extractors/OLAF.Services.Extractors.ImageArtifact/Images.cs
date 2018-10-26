using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.Services.Extractors
{
    public class Images : Service<ArtifactMessage, ImageArtifact>
    {
        public Images(Profile profile, Type[] clients) : base(profile, clients) {}

        public override ApiResult Init() => SetInitializedStatusAndReturnSucces();

        protected override ApiResult ProcessClientQueue(ArtifactMessage message)
        {
            try
            {
                Bitmap image = Accord.Imaging.Image.FromFile(message.ArtifactPath);
                Debug("Extracted image from file {0} with dimensions {1}x{2}.", message.ArtifactName, image.Width,
                    image.Height);
                Global.MessageQueue.Enqueue<Images>(new ImageArtifact(message, image));
                return ApiResult.Success;
            }
            catch (Exception e)
            {
                Error(e, "An error occurred attempting to read the image file {0}.", message.ArtifactPath);
                return ApiResult.Failure;
            }

        }
    }
}
