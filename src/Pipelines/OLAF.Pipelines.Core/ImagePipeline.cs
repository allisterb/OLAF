using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using OLAF.Services.Classifiers;
using OLAF.Services.OCR;
using OLAF.Services.Extractors;
namespace OLAF.Pipelines
{
    public class ImagePipeline : Pipeline
    {
        public ImagePipeline(Profile profile) : base(profile)
        {
            AddService<Images>();
            AddService<Tesseract>();
            AddService<ViolaJonesFaceDetector>();
            AddService<MSComputerVision>();
            SetPipelineInitializingStatus();   
        }

        public override string Description { get; } = "First pipeline";

    }
}
