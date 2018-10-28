using Leptonica;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace OLAF.Services.OCR
{
    public class TesseractOCR : Service<ImageArtifact, Message>
    {
        #region Constructors
        public TesseractOCR(Profile profile, params Type[] clients) :base(profile, clients)
        {
            try
            {
                TessBaseAPI = new TessBaseAPI();
                Info("Tesseract library version {0}.", TessBaseAPI.GetVersion());
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
                Error("Tesseract data directory tessdata not found.");
                Status = ApiStatus.FileNotFound;
                return ApiResult.Failure;
            }

            if(!TessBaseAPI.Init(GetDataDirectoryPathTo("tessdata"), "eng", OcrEngineMode.DEFAULT))
            {
                Error("Could not initialize tesseract.net API.");
                Status = ApiStatus.Error;
                return ApiResult.Failure;
            }

            TessBaseAPI.SetPageSegMode(PageSegmentationMode.AUTO_OSD);
            Status = ApiStatus.Initialized;
            Info("Tesseract OCR initialized.");
            return ApiResult.Success;
        }

        protected override ApiResult ProcessClientQueueMessage(ImageArtifact message)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Properties
        public TessBaseAPI TessBaseAPI { get; }
        #endregion

    }
}
