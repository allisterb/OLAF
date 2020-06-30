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
        public VaderSharp(Profile profile) : base(profile) {}
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
            foreach (string t in message.Text.Split(Environment.NewLine.ToCharArray()))
            {
                var k = SentimentIntensityAnalyzer.PolarityScores(t);
                message.Sentiment[t] = k.Compound;
            }
            return ApiResult.Success;
        }
        #endregion

        #region Properties
        protected SentimentIntensityAnalyzer SentimentIntensityAnalyzer { get; set; }
        #endregion
    }
}
