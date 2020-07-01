using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LogAnalytics.DataCollector.Wrapper;

namespace OLAF.Services.Storage
{
    public class AzureLogAnalytics : Service<Artifact, Artifact>
    {
        public AzureLogAnalytics(Profile profile, params Type[] clients) : base(profile, clients) 
        { 
        }

        public override ApiResult Init()
        {
            var i = Global.GetAppSetting("cred.config").Trim().Split(":".ToCharArray());
            if (i.Length != 2 || string.IsNullOrEmpty(i[0]) || string.IsNullOrEmpty(i[1]))
            {
                Error("Could not read Azure Log Analytics key or workspace id from file {0}.", "cred.config");
                return SetErrorStatusAndReturnFailure();
            }
            else
            {
                I = i[0];
                K = i[1];
            }
            Wrapper = new LogAnalyticsWrapper(I, K);
            return SetInitializedStatusAndReturnSucces();
        }

        protected override ApiResult ProcessClientQueueMessage(Artifact message)
        {
            throw new NotImplementedException();
        }

        #region Properties
        public string I { get; protected set; }

        public string K { get; protected set; }

        public LogAnalyticsWrapper Wrapper { get; protected set; }
        #endregion

    }
}
