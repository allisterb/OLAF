using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace OLAF.Services.Classifiers
{
    public class MSComputerVision : Service<ImageArtifact, ImageArtifact>
    {
        #region Constructors
        public MSComputerVision(Profile profile, params Type[] clients) : base(profile, clients)
        {
            if (ConfigurationManager.AppSettings["OLAF-MS-CV"] == null)
            {
                Error("No Microsoft Computer Vision API accounts are configured.");
                Status = ApiStatus.ConfigurationError;
            }
            else if (ConfigurationManager.AppSettings["OLAF-MS-CV-API"] == null)
            {
                Error("No Microsoft Computer Vision API accounts are configured.");
                Status = ApiStatus.ConfigurationError;
            }
            else
            {
                ApiAccountKey = ConfigurationManager.AppSettings["OLAF-MS-CV"];
                ApiEndpointUrl = ConfigurationManager.AppSettings["OLAF-MS-CV-API"];
                Status = ApiStatus.Initializing;
            }
        }
        #endregion

        #region Overridden members
        public override ApiResult Init()
        {
            try
            {
                ApiKeyServiceClientCredentials c = new ApiKeyServiceClientCredentials(ApiAccountKey);
                Client = new ComputerVisionClient(c);
                Client.Endpoint = ApiEndpointUrl;
                Status = ApiStatus.Initialized;
                Info("Microsoft Computer Vision API classifier initialized.");
                return ApiResult.Success;
            }
            catch(Exception e)
            {
                Error(e, "Error creating Microsoft Computer Vision API client.");
                Status = ApiStatus.RemoteApiClientError;
                return ApiResult.Failure;
            }
        }

        protected override ApiResult ProcessClientQueue(ImageArtifact artifact)
        {
            if (!artifact.HasDetectedObjects(ImageObjectKinds.Face))
            {
                Global.MessageQueue.Enqueue<MSComputerVision>(artifact);
                return ApiResult.Success;
            }
            else
            {
                Info("Artifact has faces detected; analyzing using MS Computer Vision API.");
                ImageAnalysis analysis = null;
                try
                {
                    using (FileStream stream = new FileStream(artifact.Artifact.Path, FileMode.Open))
                    {
                        Task<ImageAnalysis> t1 = Client.AnalyzeImageInStreamAsync(stream, null, null, null, cancellationToken);
                        analysis = t1.Result;
                    }
                }
                catch (Exception e)
                {
                    Error(e, "An error occurred during image analysis using the Microsoft Computer Vision API.");
                    return ApiResult.Failure;
                }
                return ApiResult.Success;
            }
            
        }
        #endregion

        #region Properties
        public ComputerVisionClient Client { get; protected set; }
        #endregion
    }
}
