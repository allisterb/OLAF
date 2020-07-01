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
    [Description("Process document artifacts.")]
    public class DocumentPipeline : Pipeline
    {
        public DocumentPipeline(Profile profile) : base(profile)
        {
            AddService<Documents>();
            //AddService<MSTextAnalytics>();
            AddService<AzureLogAnalytics>();
            SetPipelineInitializingStatus();
        }
    }

}
