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
                return SetInitializedStatusAndReturnSucces();
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
            if (!artifact.HasDetectedObjects(ImageObjectKinds.FaceCandidate) || artifact.HasOCRText)
            {
                Debug("Not calling MS Computer Vision API for image artifact {0} without face object candidates.", artifact.Id);
            }

            else if (artifact.FileArtifact == null)
            {
                Debug("Not calling MS Computer Vision API for non-file image artifact {0}.", artifact.Id);
            }

            else
            {
                Info("Artifact {0} is likely a photo with faces detected; analyzing using MS Computer Vision API.", artifact.Id);
                ImageAnalysis analysis = null;
                using (var op = Begin("Analyze image using MS Computer Vision API."))
                {
                    try
                    {
                        using (Stream stream = artifact.FileArtifact.HasData ? (Stream) new MemoryStream(artifact.FileArtifact.Data)
                            : new FileStream(artifact.FileArtifact.Path, FileMode.Open))
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

                        artifact.Categories.Add(new ArtifactCategory(c.Name, null, c.Score));
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
            }
            return ApiResult.Success;
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
