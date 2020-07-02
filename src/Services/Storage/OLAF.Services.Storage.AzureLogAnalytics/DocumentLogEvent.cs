using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.Services.Storage
{
    public class DocumentLogEvent : BaseLogEvent
    {
        public DocumentLogEvent() { }
        public DocumentLogEvent(TextArtifact artifact) :base(artifact)
        {
            Name = artifact.Name;
            KeyWords = string.Join(",", artifact.KeyWords.ToArray());
            Entities = string.Join(",", artifact.Entities.ToArray());
        }

        public DocumentLogEvent(ImageArtifact artifact) : base(artifact)
        {
            Name = artifact.Name;
            Categories = string.Join(",", artifact.Categories.Select(c => c.Name).ToArray());
            Tags = string.Join(",", artifact.Tags);
        }
        
        public string Name { get; set; }

        public string FilePath { get; set; }

        public string KeyWords { get; set; }

        public string Entities { get; set; }

        public string Categories { get; set; }

        public string Tags { get; set; }

        public string Caption { get; set; }

    }
}
