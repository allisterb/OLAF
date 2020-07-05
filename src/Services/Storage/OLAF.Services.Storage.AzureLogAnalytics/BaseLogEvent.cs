using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF.Services.Storage
{
    public class BaseLogEvent
    {
        public BaseLogEvent(Artifact artifact)
        {
            Name = artifact.HasFileSource ? System.IO.Path.GetFileName(artifact.Name) : artifact.Name;
            UserName = artifact.UserName;
            EventTime = artifact.CreationTime;
            Application = artifact.CurrentWindowTitle;
        }
        public string Severity { get; set; }
        
        public string Name { get; set; }

        public string UserName { get; set; }

        public string ComputerName { get; set; } = Environment.MachineName;
        
        public string Message { get; set; }
        
        public string Source { get; set; }
        
        public string Category { get; set; }
        
        public string ArtifactType { get; set; }

        public DateTime EventTime { get; set; }

        public string Application { get; set; }
    }
}
