using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OLAF
{
    public class Artifact : Message
    {
        public Artifact(long id, string artifactPath) : base(id)
        {
            Path = artifactPath;
        }

        #region Properties
        public string Path { get; }
        public string Name => Path?.GetPathFilename();
        public bool FileLocked { get; set; } = false;
        public int FileExtractAttempts { get; set; }
        #endregion
    }
}
