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
            string vk, ve;
            if (string.IsNullOrEmpty(vk = Global.GetAppSetting("cred.config", "VK")))
            {
                Error("Could not read the Azure Computer Vision key from file {0}.", "cred.config");
                Status = ApiStatus.ConfigurationError;
            }
            else if (string.IsNullOrEmpty(ve = Global.GetAppSetting("cred.config", "VE")))
            {
                Error("Could not read the Azure Computer Vision endpoint URL from file {0}.", "cred.config");
                Status = ApiStatus.ConfigurationError;
            }
            else
            {
                ApiAccountKey = vk;
                ApiEndpointUrl = ve;
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
                Error(e, "Error creating Microsoft Azure Vision API client.");
                Status = ApiStatus.RemoteApiClientError;
                return ApiResult.Failure;
            }
        }

        protected override ApiResult ProcessClientQueueMessage(ImageArtifact artifact)
        {
            ImageAnalysis analysis = null;
            using (var op = Begin("Analyze image using the Azure Computer Vision API"))
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
                    Error(e, "An error occurred during image analysis using the Azure Computer Vision API.");
                    return ApiResult.Failure;
                }
            }
            if (analysis.Categories != null)
            {
                foreach (Category c in analysis.Categories)
                {
                    artifact.Categories.Add(new ArtifactCategory(c.Name, null, c.Score));
                }
                Info("Azure Computer Vision returned {0} categories.", analysis.Categories.Count);
                Debug("Image categories: {0}", analysis.Categories.Select(c => c.Name + "/" + c.Score.ToString()));
            }
            Debug("Image properties: Adult: {0}/{1} Racy: {2}/{3} Tags: {4}, Captions: {5}.",
                analysis.Adult.IsAdultContent, analysis.Adult.AdultScore, analysis.Adult.IsRacyContent,
                analysis.Adult.RacyScore, analysis.Description.Tags, analysis.Description.Captions.Where(c => c.Confidence > 0.6).Select(c => c.Text));

            artifact.IsAdultContent = analysis.Adult.IsAdultContent;
            artifact.AdultContentScore = analysis.Adult.AdultScore;
            artifact.IsRacy = analysis.Adult.IsRacyContent;
            artifact.RacyContentScore = analysis.Adult.RacyScore;
            artifact.Captions = analysis.Description.Captions.Where(c => c.Confidence > 0.6).Select(c => c.Text).ToList();
            artifact.Tags = analysis.Description.Tags.ToList();
            return ApiResult.Success;
        }
        #endregion

        #region Properties
        public ComputerVisionClient Client { get; protected set; }
        public List<VisualFeatureTypes> VisualFeatures { get; } = new List<VisualFeatureTypes>()
        {
            VisualFeatureTypes.Categories,
            VisualFeatureTypes.Description,
            VisualFeatureTypes.Adult,
            VisualFeatureTypes.Faces,
            VisualFeatureTypes.Tags,
        };
        
        #endregion
    }
}
