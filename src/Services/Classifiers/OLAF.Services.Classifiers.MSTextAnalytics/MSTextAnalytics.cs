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
            string ak, ae;
            //using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models.
            if (string.IsNullOrEmpty(ak = Global.GetAppSetting("cred.config", "AK")))
            {
                Error("Could not read the Azure Text Analytics key from file {0}.", "cred.config");
                Status = ApiStatus.ConfigurationError;
            }
            else if (string.IsNullOrEmpty(ae = Global.GetAppSetting("cred.config", "AE")))
            {
                Error("Could not read the Azure Text Analytics endpoint URL from file {0}.", "cred.config");
                Status = ApiStatus.ConfigurationError;
            }
            else
            { 
                ApiAccountKey = ak;
                ApiEndpointUrl = ae;
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

            var _text = artifact.Text.Split(Environment.NewLine.ToCharArray());
            List<MultiLanguageInput> mlinput = _text.Select((t, i) => new MultiLanguageInput("en", i.ToString(), t)).ToList();
            Info("Analyzing text artifact {0} using MS Text Analytics.", artifact.Id);
            EntitiesBatchResultV2dot1 entitiesResult;
            KeyPhraseBatchResult keyPhraseResult;
            using (var op = Begin("Detect sentiment and entities using MS Text Analytics API."))
            {
                try
                {
                    Task<EntitiesBatchResultV2dot1> er = Client.EntitiesAsync(new MultiLanguageBatchInput(mlinput));
                    Task<KeyPhraseBatchResult> kr = Client.KeyPhrasesAsync(new MultiLanguageBatchInput(mlinput));
                    Task.WaitAll(er, kr);
                    keyPhraseResult = kr.Result;
                    entitiesResult = er.Result;
                    op.Complete();
                    Info("Text artifact {0} summary:", artifact.Id);
                    
                    for (int i = 0; i < _text.Count(); i++)
                    {
                        string s = _text[i];
                        Info("Entities: {0}. Keywords: {1}.",
                            entitiesResult.Documents.SingleOrDefault(d => d.Id == i.ToString())?.Entities.Select(e => e.Name),
                            keyPhraseResult.Documents.SingleOrDefault(d => d.Id == i.ToString())?.KeyPhrases.Select(e => e));
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
