using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;

namespace OLAF.Services
{
    public class AzureStorageBlobUpload : 
        Service<ArtifactMessage, AzureStorageBlobUploadedMessage>, IService
    {
        #region Constructors
        public AzureStorageBlobUpload(Profile profile, params Type[] clients) : base(profile, clients)
        {
            if (ConfigurationManager.ConnectionStrings["OLAFArtifacts"] == null)
            {
                UseEmulator = true;
                ApiConnectionString = "UseDevelopmentStorage=true";
                Info("Using Azure Storage Emulator.");
            }
            else
            {
                ApiConnectionString = ConfigurationManager.ConnectionStrings["OLAFArtifacts"].ConnectionString;
            }
            Status = ApiStatus.Initializing;
        }
        #endregion

        #region Overridden members
        public override ApiResult Init()
        {
            ThrowIfNotInitializing();
            Storage = new AzureStorageApi(ApiConnectionString);
            if (Storage.Initialised)
            {
                Status = ApiStatus.Initialized;
                return ApiResult.Success;
            }
            else
            {
                Status = ApiStatus.Error;
                return ApiResult.Failure;
            }
        }

        protected override ApiResult ProcessClientQueue(ArtifactMessage message)
        {
            ThrowIfNotOk();
            CloudBlockBlob blob = null;
            string containerName = GetAzureResourceName(Profile.Name).ToLower();
            string blobName = GetAzureResourceName(message.ArtifactPath.GetPathFilename());
            try
            {
                Task<CloudBlob> t = Storage.GetorCreateCloudBlobAsync(containerName, blobName,
                    BlobType.BlockBlob);
                blob = (CloudBlockBlob)t.Result;
                Global.MessageQueue.Enqueue<AzureStorageBlobUpload>(
                    new AzureStorageBlobUploadedMessage(message.Id, blob));
                return ApiResult.Success;
            
            }
            catch (Exception e)
            {
                Error(e, "Error occurred attempting to upload artifact {0} to container {1}",
                    message.ArtifactName, Profile.Name);
                return ApiResult.Failure;
            }
        }
        #endregion

        #region Methods
        public AzureStorageApi Storage { get; protected set; }

        protected bool UseEmulator { get; }

        public static string GetAzureResourceName(string name)
        {
            StringBuilder rn = new StringBuilder(63, 63);
            int i = 0;
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c))
                    rn.Append(c);
                else
                    rn.Append('-');
                if (++i == 63) break;
            }
            return rn.ToString();
        }
        #endregion
    }
}
