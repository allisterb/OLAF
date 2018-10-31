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

        protected override ApiResult ProcessClientQueueMessage(TextArtifact message)
        {
            List<Input> input = message.Text.Select((t, i) => new Input(i.ToString(), t)).ToList();
            LanguageBatchResult result;
            using (var op = Begin("Find English language sentences using MS Text Analytics API."))
            {
                try
                {
                    Task<LanguageBatchResult> r = Client.DetectLanguageAsync(new BatchInput(input));
                    result = r.Result;
                    op.Complete();
                }
                catch (Exception e)
                {
                    Error(e, "An error occurred connecting to the MS tText Analytics API.");
                    return SetErrorStatusAndReturnFailure();
                }
            }
            var l =  result.Documents.Where(doc => doc.DetectedLanguages.Select(d => d.Name).Contains("en"));
            Debug("Pipeline ending for artifact {0}.", message.Id);
            Info("Docs: {0}.", l);
            return SetOkStatusAndReturnSucces();
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
