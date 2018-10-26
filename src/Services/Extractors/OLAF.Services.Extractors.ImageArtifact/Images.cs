using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.Services.Extractors
{
    public class Images : Service<Artifact, ImageArtifact>
    {
        public Images(Profile profile, Type[] clients) : base(profile, clients) {}

        public override ApiResult Init() => SetInitializedStatusAndReturnSucces();

        protected override ApiResult ProcessClientQueue(Artifact message)
        {
            try
            {
                using (var op = Begin("Extracting image from artifact {0}", message.Id))
                {
                    Bitmap image = Accord.Imaging.Image.FromFile(message.Path);
                    Debug("Extracted image from file {0} with dimensions {1}x{2}.", message.Name, image.Width,
                        image.Height);
                    Global.MessageQueue.Enqueue<Images>(new ImageArtifact(message, image));
                    op.Complete();
                }
                return ApiResult.Success;
            }
            catch (Exception e)
            {
                Error(e, "An error occurred attempting to read the image file {0}.", message.Path);
                return ApiResult.Failure;
            }

        }
    }
}
