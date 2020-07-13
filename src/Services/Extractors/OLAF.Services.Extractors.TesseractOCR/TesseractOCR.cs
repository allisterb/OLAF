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
                    string ant = TextArtifact.GetAlphaNumericString(resultIterator.GetUTF8Text(pageIteratorLevel));
                    ant = string.Join(" ",
                        ant.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(word => TextArtifact.IsNumber(word) || word.Length > 3 || Pipeline.Dictionaries["common_words_en_3grams"].Contains(word)))
                        .Trim();
                    if (ant.IsEmpty())
                    {
                        continue;
                    }
                    else
                    {
                        text.Add(ant);
                    }
                }
                while (resultIterator.Next(pageIteratorLevel));

                if (text.Count > 0)
                {
                    string alltext = text.Aggregate((s1, s2) => s1 + " " + s2).Trim();
                    if (text.Count < 7)
                    {
                        Info("Artifact id {0} is likely a photo or non-text image.", message.Id);
                    }
                    else
                    {
                        message.OCRText = text;
                        Info("OCR Text: {0}", alltext);
                    }
                }
                else
                {
                    Info("No text recognized in artifact id {0}.", message.Id);
                }
                op.Complete();
            }

            message.Image.UnlockBits(bData);
            if (text.Count >= 7)
            {
                TextArtifact artifact = new TextArtifact(message.Name + ".txt", string.Join(Environment.NewLine, text.ToArray()));
                artifact.Source = message.Source;
                artifact.CurrentProcess = message.CurrentProcess;
                artifact.CurrentWindowTitle = message.CurrentWindowTitle;
                artifact.Image = message;
                message.TextArtifact = artifact;
                EnqueueMessage(artifact);
                Info("{0} added artifact id {1} of type {2} from artifact {3}.", Name, artifact.Id, artifact.GetType(), 
                    message.Id);
            }
            return ApiResult.Success;
        }
        #endregion

        #region Properties
        public TessBaseAPI TesseractImage { get; }
        public Pix Pix { get; protected set; }
        #endregion

        #region Methods
        protected static string RemoveBigrams(string l)
        {
            return l.Split('\r', '\n', ' ').Select(w => w.Trim()).Where(w => w.Length > 2).Aggregate((s1, s2) => s1 + " " + s2);
        }
        #endregion
    }
}
