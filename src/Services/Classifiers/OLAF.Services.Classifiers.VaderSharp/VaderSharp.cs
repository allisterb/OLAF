using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VaderSharp;
namespace OLAF.Services.Classifiers
{
    public class VaderSharp : Service<TextArtifact, TextArtifact>
    {
        #region Constructors
        public VaderSharp(Profile profile) : base(profile)
        {
            if (Status != ApiStatus.Initializing)
            {
                return;
            }
            Status = ApiStatus.Initializing;
        }
        #endregion

        #region Overriden members
        public override ApiResult Init()
        {
            if (Status != ApiStatus.Initializing)
            {
                SetErrorStatusAndReturnFailure();
            }

            SentimentIntensityAnalyzer = new SentimentIntensityAnalyzer();
            return SetInitializedStatusAndReturnSucces();
        }

        protected override ApiResult ProcessClientQueueMessage(TextArtifact message)
        {
            foreach (string t in message.Text)
            {
                var k = SentimentIntensityAnalyzer.PolarityScores(t);
                message.Sentiment[t] = k.Compound;
            }
            Info("VaderSharp sentiment scores: {0}.", message.Sentiment);
            EnqueueMessage(message);
            return ApiResult.Success;
            
        }
        #endregion

        #region Properties
        protected SentimentIntensityAnalyzer SentimentIntensityAnalyzer { get; set; }
        #endregion
    }
}
