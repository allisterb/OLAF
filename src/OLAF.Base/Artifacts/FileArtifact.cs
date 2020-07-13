using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OLAF
{
    public enum UserFileOperation
    {
        CREATE,
        DOWNLOAD,
        COPY_EXTERNAL
    }
    public class FileArtifact : Artifact
    {
        #region Constructors
        public FileArtifact(long id, string artifactPath, string originalPath) : base(id)
        {
            Path = artifactPath;
            base.Name = Path?.GetPathFilename();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Drive = DriveInfo.GetDrives().FirstOrDefault(d => originalPath.Substring(0, 3) == d.Name);
            }
            if (Drive != null && Drive.DriveType != DriveType.Fixed)
            {
                UserOp = UserFileOperation.COPY_EXTERNAL;
            }
            else
            {
                UserOp = UserFileOperation.CREATE;
            }
        }

        public FileArtifact(long id, string artifactPath, string originalPath, byte[] artifactFileData) : this(id, artifactPath, originalPath)
        {
            Data = artifactFileData;
        }
        #endregion

        #region Properties
        public string Path { get; }

        public DriveInfo Drive { get; }
        public bool FileLocked { get; set; } = false;
        public int FileOpenAttempts { get; set; } = 0;
        public byte[] Data { get; set; }
        public bool HasData => Data != null && Data.Length > 0;
        public UserFileOperation UserOp { get; set; }
        #endregion
    }
}
