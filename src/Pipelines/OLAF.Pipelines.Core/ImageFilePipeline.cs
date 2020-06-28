using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using OLAF.Services.Classifiers;
using OLAF.Services.Extractors;
using OLAF.Services.Storage;

namespace OLAF.Pipelines
{
    [Description("Process image file artifacts.")]
    public class ImageFilePipeline : Pipeline
    {
        public ImageFilePipeline(Profile profile) : base(profile)
        {
            AddService<Images>();
            AddService<TesseractOCR>();
            AddService<ViolaJonesFaceDetector>();
            AddService<MSComputerVision>();
            AddService<AzureStorageBlobUpload>();
            SetPipelineInitializingStatus();
        }
    }
    
}
