using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Rest;

namespace OLAF.Services.Classifiers
{
    public class MSTextAnalytics : Service<TextArtifact, TextArtifact>
    {
        #region Constructors
        public MSTextAnalytics(Profile profile) : base(profile)
        {
            if (ConfigurationManager.AppSettings["OLAF-MS-TXT"] == null)
            {
                Error("No Microsoft Text Analytics API accounts are configured.");
                Status = ApiStatus.ConfigurationError;
            }
            else if (ConfigurationManager.AppSettings["OLAF-MS-TXT-API"] == null)
            {
                Error("No Microsoft Text Analytics API accounts are configured.");
                Status = ApiStatus.ConfigurationError;
            }
            else
            {
                ApiAccountKey = ConfigurationManager.AppSettings["OLAF-MS-TXT"];
                ApiEndpointUrl = ConfigurationManager.AppSettings["OLAF-MS-TXT-API"];
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
                Client = new TextAnalyticsClient(c);
                Client.Endpoint = ApiEndpointUrl;
                return SetInitializedStatusAndReturnSucces();
            }
            catch (Exception e)
            {
                Error(e, "Error creating Microsoft Computer Vision API client.");
                return SetErrorStatusAndReturnFailure();
            }
        }

        protected override ApiResult ProcessClientQueueMessage(TextArtifact artifact)
        {
            List<MultiLanguageInput> mlinput = artifact.Text
                .Where(t => artifact.HasHatePhrases[t].Value || artifact.HasIdentityHateWords[t].Value)
                .Select((t, i) => new MultiLanguageInput("en", i.ToString(), t)).ToList();

            if (mlinput.Count == 0)
            {
                Info("No hate speech detected.");
                return ApiResult.Success;
            }

            Info("Analyzing text artifact {0} with hate words using MS Text Analytics.", artifact.Id);
            SentimentBatchResult sentimentResult;
            EntitiesBatchResultV2dot1 entitiesResult;
            using (var op = Begin("Detect sentiment and entities using MS Text Analytics API."))
            {
                try
                {
                    Task<SentimentBatchResult> sr = Client.SentimentAsync(new MultiLanguageBatchInput(mlinput));
                    Task<EntitiesBatchResultV2dot1> er = Client.EntitiesAsync(new MultiLanguageBatchInput(mlinput));
                    Task.WaitAll(sr, er);
                    sentimentResult = sr.Result;
                    entitiesResult = er.Result;
                    op.Complete();
                    Info("Text artifact {0} summary:", artifact.Id);
                    for (int i = 0; i < artifact.Text.Count; i++)
                    {
                        string s = artifact.Text[i];
                        Info("{0}. Text: {1}. Has Profanity: {2}. Has IdentityHate: {3}. VADER Sentiment: {4}. MS Text Sentiment: {5}. Entities: {6}.",
                            i, s, artifact.HasProfanity[s], artifact.HasIdentityHateWords[s],
                            artifact.Sentiment[s],
                            sentimentResult.Documents.SingleOrDefault(d => d.Id == i.ToString())?.Score,
                            entitiesResult.Documents.SingleOrDefault(d => d.Id == i.ToString())?.Entities.Select(e => e.Name));
                    }
                    return ApiResult.Success;
                }
                catch (Exception e)
                {
                    Error(e, "An error occurred connecting to the MS tText Analytics API.");
                    return ApiResult.Failure;
                }
            }
        }

        #endregion

        #region Properties
        TextAnalyticsClient Client { get; set; }
        #endregion

        #region Types
        class ApiKeyServiceClientCredentials : ServiceClientCredentials
        {
            public ApiKeyServiceClientCredentials(string s)
            {
                subscriptionKey = s;
            }

            public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                return base.ProcessHttpRequestAsync(request, cancellationToken);
            }

            protected string subscriptionKey;
        }
        #endregion
    }
}
