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

        protected override ApiResult ProcessClientQueueMessage(ImageArtifact artifact)
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
                using (var op = Begin("Analyze image using MS Computer Vision API."))
                {
                    try
                    {
                        using (FileStream stream = new FileStream(artifact.Artifact.Path, FileMode.Open))
                        {
                            Task<ImageAnalysis> t1 = Client.AnalyzeImageInStreamAsync(stream,
                                VisualFeatures, null, null, cancellationToken);
                            analysis = t1.Result;
                            op.Complete();
                        }
                    }
                    catch (Exception e)
                    {
                        Error(e, "An error occurred during image analysis using the Microsoft Computer Vision API.");
                        return ApiResult.Failure;
                    }
                }
                if (analysis.Categories != null)
                {
                    Info("Image categories: {0}", analysis.Categories.Select(c => c.Name + "/" + c.Score.ToString()));
                    foreach (Category c in analysis.Categories)
                    {

                        artifact.Catgegories.Add(new ArtifactCategory(c.Name, null, c.Score));
                    }
                }
                Info("Image properties: Adult: {0}/{1} Racy: {2}/{3} Description:{4}",
                    analysis.Adult.IsAdultContent, analysis.Adult.AdultScore, analysis.Adult.IsRacyContent,
                    analysis.Adult.RacyScore, analysis.Description.Tags);
                artifact.IsAdultContent = analysis.Adult.IsAdultContent;
                artifact.AdultContentScore = analysis.Adult.AdultScore;
                artifact.IsRacy = analysis.Adult.IsRacyContent;
                artifact.RacyContentScore = analysis.Adult.RacyScore;
                analysis.Description = analysis.Description;

                Global.MessageQueue.Enqueue<MSComputerVision>(artifact);
                return ApiResult.Success;
            }
            
        }
        #endregion

        #region Properties
        public ComputerVisionClient Client { get; protected set; }
        public List<VisualFeatureTypes> VisualFeatures { get; } = new List<VisualFeatureTypes>()
        {
            VisualFeatureTypes.Categories,
            VisualFeatureTypes.Description,
            VisualFeatureTypes.Adult
        };
        
        #endregion
    }
}
