using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leptonica;
using Tesseract;

namespace OLAF.Services.OCR
{
    public class TesseractOCR : Service<ImageArtifact, ImageArtifact>
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
                Error("Tesseract data directory not found.");
                Status = ApiStatus.FileNotFound;
                return ApiResult.Failure;
            }

            if(!TesseractImage.Init(GetDataDirectoryPathTo("tessdata"), "eng", OcrEngineMode.DEFAULT, 
                new[] { "logfile" }))
            {
                Error("Could not initialize tesseract.net.");
                Status = ApiStatus.Error;
                return ApiResult.Failure;
            }

            TesseractImage.SetPageSegMode(PageSegmentationMode.AUTO_OSD);
            Status = ApiStatus.Initialized;
            Info("Tesseract OCR initialized.");
            return ApiResult.Success;
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
            using (var op = Begin("Tesseract OCR (fast)"))
            {
                TesseractImage.Recognize();
                ResultIterator resultIterator = TesseractImage.GetIterator();
                StringBuilder stringBuilder = new StringBuilder();
                PageIteratorLevel pageIteratorLevel = PageIteratorLevel.RIL_PARA;
                do
                {
                    stringBuilder.Append(resultIterator.GetUTF8Text(pageIteratorLevel).Trim());
                }
                while (resultIterator.Next(pageIteratorLevel));

                string text = stringBuilder.ToString();
                if (string.IsNullOrEmpty(text) || text.Length < 20)
                {
                    Info("{0} is likely a photo or non-text image.", message.FileArtifact.Name);
                }
                else
                {
                    message.OCRText = text;
                    Info("OCR Text: {0}", message.OCRText);
                }
                op.Complete();
            }
            message.Image.UnlockBits(bData);
            Global.MessageQueue.Enqueue<TesseractOCR>(message);
            return ApiResult.Success;
        }
        #endregion

        #region Properties
        public TessBaseAPI TesseractImage { get; }
        public Pix Pix { get; protected set; }
        #endregion
    }
}
