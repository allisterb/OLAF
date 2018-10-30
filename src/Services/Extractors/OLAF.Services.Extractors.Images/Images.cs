using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;

namespace OLAF.Services.Extractors
{
    public class Images : Service<FileArtifact, ImageArtifact>
    {
        public Images(Profile profile, params Type[] clients) : base(profile, clients) {}

        public override ApiResult Init() => SetInitializedStatusAndReturnSucces();

        protected override ApiResult ProcessClientQueueMessage(FileArtifact artifact)
        {
            if (!artifact.HasData)
            {
                try
                {
                    artifact.FileOpenAttempts++;
                    using (var op = Begin("Extracting image from artifact {0}", artifact.Id))
                    {
                        Bitmap image = Accord.Imaging.Image.FromFile(artifact.Path);
                        Debug("Extracted image from file {0} with dimensions: {1}x{2} pixel format: {3} hres: {4} yres: {5}.",
                            artifact.Name, image.Width, image.Height, image.PixelFormat, image.HorizontalResolution,
                            image.VerticalResolution);
                        op.Complete();
                        Global.MessageQueue.Enqueue<Images>(new ImageArtifact(artifact, image));
                    }
                    return ApiResult.Success;
                }
                catch (ArgumentException ae)
                {
                    if (artifact.FileLocked && artifact.FileOpenAttempts > 5)
                    {
                        Error(ae, "{0} file locked for more than 5 attempts or other error during extraction. Aborting extract attempt.", artifact.Name);
                        return ApiResult.Failure;
                    }
                    else if (ae.Message.Contains("Parameter is not valid"))
                    {
                        artifact.FileLocked = true;
                    }

                }
                catch (Exception e)
                {
                    Error(e, "An error occurred attempting to read the image file {0}.", artifact.Path);
                    return ApiResult.Failure;
                }

                if (artifact.FileLocked && artifact.FileOpenAttempts <= 5)
                {
                    Debug("{0} file may be locked...pausing a bit and trying extraction again...({1})", artifact.Name,
                        artifact.FileOpenAttempts);
                    Thread.Sleep(100);
                    return ProcessClientQueueMessage(artifact);
                }
                else if (artifact.FileLocked && artifact.FileOpenAttempts > 5)
                {
                    Error("{0} file locked for more than 5 attempts or other error during extraction. Aborting extract attempt.", artifact.Name);
                    return ApiResult.Failure;
                }
                else
                {
                    Error("Unknown error extracting image from {0}; aborting extract attempt.", artifact.Name);
                    return ApiResult.Failure;
                }
            }
            else
            {
                using (var op = Begin("Extracting image from artifact {0}", artifact.Id))
                {
                    Bitmap image = (Bitmap) Image.FromStream(new MemoryStream(artifact.Data));
                    Debug("Extracted image from file {0} with dimensions: {1}x{2} pixel format: {3} hres: {4} yres: {5}.",
                           artifact.Name, image.Width, image.Height, image.PixelFormat, image.HorizontalResolution,
                           image.VerticalResolution);
                    op.Complete();
                    Global.MessageQueue.Enqueue<Images>(new ImageArtifact(artifact, image));
                }
                return ApiResult.Success;
            }
        }

       
    }
}
