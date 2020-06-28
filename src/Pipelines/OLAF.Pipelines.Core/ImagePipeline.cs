using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using OLAF.Services.Classifiers;
using OLAF.Services.Extractors;

namespace OLAF.Pipelines
{
    [Description("Process file and app window activity image artifacts.")]
    public class ImagePipeline : Pipeline
    {
        public ImagePipeline(Profile profile) : base(profile)
        {
            AddService<Images>();
            AddService<TesseractOCR>();
            AddService<ViolaJonesFaceDetector>();
            AddService<MSComputerVision>();
            AddService<VaderSharp>();
            AddService<MSTextAnalytics>();
            SetPipelineInitializingStatus();   
        }
    }
}
