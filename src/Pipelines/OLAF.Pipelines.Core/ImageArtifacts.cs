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
    public class ImageArtifacts : Pipeline
    {
        public ImageArtifacts(Profile profile) : base(profile)
        {
            AddService<FileImages>();
            AddService<TesseractOCR>();
            AddService<ViolaJonesFaceDetector>();
            AddService<MSComputerVision>();
            AddService<AzureStorageBlobUpload>();
            SetPipelineInitializingStatus();
        }
    }
    
}
