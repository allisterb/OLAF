using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.Services.Storage
{
    public class AzureLogAnalytics : Service<Artifact, Artifact>
    {
        public AzureLogAnalytics(Profile profile, params Type[] clients) : base(profile, clients) 
        { 
        }


    }
}
