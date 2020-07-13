using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.Services.Storage
{
    public class DocumentLogEvent : BaseLogEvent
    {
        public DocumentLogEvent(TextArtifact artifact) : base(artifact)
        {
            PotentialSensitiveData = artifact.HasSensitiveData ? artifact.SensitiveData.Values.Aggregate((p, n) => p + "," + n) : "";
            CompetitorsNamePresent = artifact.CompetitorNamesPresent.Count > 0 ? artifact.CompetitorNamesPresent.Aggregate((p, n) => p + "," + n) : "";
            KeyWords = string.Join(",", artifact.KeyWords.ToArray());
            Entities = string.Join(",", artifact.Entities.ToArray());
            UserOp = artifact.HasFileSource ? (artifact.Source as FileArtifact).UserOp.ToString() : string.Empty;
            Global.Logger.Debug("Created Azure Log Analytics log event {0} for text artifact {1} from user op {2} at {3}.", Name, artifact.Id, UserOp, DateTime.Now);
        }

        public DocumentLogEvent(ImageArtifact artifact) : base(artifact)
        {
            IsImage = true;
            Categories = string.Join(",", artifact.Categories.Select(c => c.Name).ToArray());
            Tags = string.Join(",", artifact.Tags);
            IsAdultImage = artifact.IsAdultContent || artifact.IsRacy;
            UserOp = artifact.HasFileSource ? (artifact.Source as FileArtifact).UserOp.ToString() : string.Empty;
            Global.Logger.Debug("Created Azure Log Analytics log event {0} for image artifact {1} from user op {2} at {3}.", Name, artifact.Id, UserOp, DateTime.Now);
        }

        public string FilePath { get; set; }

        public string UserOp { get; set; }

        public bool IsImage { get; set; }

        public string PotentialSensitiveData { get; set; }

        public string CompetitorsNamePresent { get; set; }

        public string KeyWords { get; set; }

        public string Entities { get; set; }

        public string Categories { get; set; }

        public string Tags { get; set; }

        public bool IsAdultImage {get; set;}
        
        public string Caption { get; set; }

    }
}
