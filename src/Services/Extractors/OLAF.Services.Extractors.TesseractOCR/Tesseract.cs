using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Leptonica;
using Tesseract;

namespace OLAF.Services.Extractors
{
    public class TesseractOCR : Service<ImageArtifact, Artifact>
    {
        #region Constructors
        public TesseractOCR(Profile profile, params Type[] clients) :base(profile, clients)
        {
            try
            {
                TesseractImage = new TessBaseAPI();
                Info("Tesseract library version is {0}.", TesseractImage.GetVersion());
                Status = ApiStatus.Initializing;
            }
            catch (Exception e)
            {
                Error(e, "Could not load tesseract.net library.");
                Status = ApiStatus.LibraryError;
            }
        }
        #endregion

        #region Overriden members
        public override ApiResult Init()
        {
            if (Status != ApiStatus.Initializing) return ApiResult.Failure;

            if (!Directory.Exists(GetDataDirectoryPathTo("tessdata")))
            {
                return SetErrorStatusAndReturnFailure("Tesseract data directory not found. Could not initialize tesseract.net.");
            }

            if (!TesseractImage.Init(GetDataDirectoryPathTo("tessdata"), "eng", OcrEngineMode.DEFAULT, 
                new[] { "logfile" }))
            {
                return SetErrorStatusAndReturnFailure("Init() method returned false. Could not initialize tesseract.net.");
            }

            TesseractImage.SetPageSegMode(PageSegmentationMode.AUTO_OSD);

            return SetInitializedStatusAndReturnSucces();
        }

        protected override ApiResult ProcessClientQueueMessage(ImageArtifact message)
        {
            BitmapData bData = message.Image.LockBits(
                new Rectangle(0, 0, message.Image.Width, message.Image.Height), ImageLockMode.ReadOnly, message.Image.PixelFormat);
            int w = bData.Width, h = bData.Height, bpp = Image.GetPixelFormatSize(bData.PixelFormat) / 8;
            unsafe
            {
                TesseractImage.SetImage(new UIntPtr(bData.Scan0.ToPointer()), w, h, bpp, bData.Stride);
            }
            Pix = TesseractImage.GetInputImage();
            
            Debug("Pix has width: {0} height: {1} depth: {2} xres: {3} yres: {4}.", Pix.Width, Pix.Height, Pix.Depth, 
                Pix.XRes, Pix.YRes);
            List<string> text;
            using (var op = Begin("Tesseract OCR (fast)"))
            {
                TesseractImage.Recognize();
                ResultIterator resultIterator = TesseractImage.GetIterator();
                text = new List<string>();
                PageIteratorLevel pageIteratorLevel = PageIteratorLevel.RIL_PARA;
                do
                {
                    string r = resultIterator.GetUTF8Text(pageIteratorLevel);
                    if (r.IsEmpty()) continue;
                    text.Add(r.Trim());
                }
                while (resultIterator.Next(pageIteratorLevel));

                if (text.Count > 0)
                {
                    string alltext = text.Aggregate((s1, s2) => s1 + " " + s2).Trim();

                    if (text.Count < 7)
                    {
                        Info("Artifact {0} is likely a photo or non-text image.", message.Id);
                    }
                    else
                    {
                        message.OCRText = text;
                        Info("OCR Text: {0}", alltext);
                    }
                }
                else
                {
                    Info("No text recognized in artifact {0}.", message.Id);
                }
                op.Complete();
            }

            message.Image.UnlockBits(bData);
            if (text.Count >= 7)
            {
                TextArtifact artifact = new TextArtifact(message.Id + 1000, text);
                EnqueueMessage(artifact);
            }

            return ApiResult.Success;
        }
        #endregion

        #region Properties
        public TessBaseAPI TesseractImage { get; }
        public Pix Pix { get; protected set; }
        #endregion
    }
}
