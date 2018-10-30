using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using OLAF.Services.OCR;

namespace OLAF.Pipelines
{
    public class AppWindowPipeline : Pipeline
    {
        public AppWindowPipeline(Profile profile) : base(profile)
        {
            AddService<Tesseract>();
            SetPipelineInitializingStatus();   
        }

        public override string Description { get; } = "Process window captures";

    }
}
