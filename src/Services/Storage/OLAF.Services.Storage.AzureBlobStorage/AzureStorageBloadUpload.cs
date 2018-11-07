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
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;

namespace OLAF.Services.Storage
{
    public class AzureStorageBlobUpload : 
        Service<Artifact, Artifact>
    {
        #region Constructors
        public AzureStorageBlobUpload(Profile profile, params Type[] clients) : base(profile, clients)
        {
            if (ConfigurationManager.ConnectionStrings["OLAFArtifacts"] == null)
            {
                UseEmulator = true;
                ApiConnectionString = "UseDevelopmentStorage=true";
                Info("{0} service using Azure Storage Emulator.", typeof(AzureStorageBlobUpload).Name);
            }
            else
            {
                ApiConnectionString = ConfigurationManager.ConnectionStrings["OLAFArtifacts"].ConnectionString;
            }
            ArtifactsBlobDrectory = AzureStorageApi.GetValidAzureBlobName(Profile.ArtifactsDirectory.Name);
            Status = ApiStatus.Initializing;
        }
        #endregion

        #region Overridden members
        public override ApiResult Init()
        {
            if (Status != ApiStatus.Initializing) return ApiResult.Failure;
            Storage = new AzureStorageApi(ApiConnectionString);
            if (Storage.Initialised)
            {
                ApiAccountName = Storage.StorageAccount.Credentials.AccountName;
                Info("{0} service initialized using Azure Blob Storage account {1}.", type.Name, ApiAccountName);
                return SetInitializedStatusAndReturnSucces();
            }
            else
            {
                return SetErrorStatusAndReturnFailure("Could not initialize {0} service using Azure Blob Storage account {1}.".F(type.Name, ApiAccountName));
            }
        }

        protected override ApiResult ProcessClientQueueMessage(Artifact artifact)
        {
            ThrowIfNotOk();

            if (!artifact.Preserve)
            {
                Info("Artifact not tagged for preservation.");
                return ApiResult.Success;
            }

            switch (artifact)
            {
                case FileArtifact fileArtifact: return UploadFileArtifact(fileArtifact);
                
                default: throw new NotImplementedException();
            }
           
        }
        #endregion

        #region Properties
        public static string ArtifactsContainerName { get; } = AzureStorageApi.GetValidAzureContainerName("olafartifacts");

        public string ArtifactsBlobDrectory { get; }

        public AzureStorageApi Storage { get; protected set; }

        protected bool UseEmulator { get; }
        #endregion

        #region Methods
        protected ApiResult UploadFileArtifact(FileArtifact artifact)
        {
            CloudBlockBlob blob = null;
            string blobName = AzureStorageApi.GetValidAzureBlobName(artifact.Name);
            using (var op = Begin("Uploading artifact {0} to Azure Blob Storage.", artifact.Name))
            {
                try
                {
                    blob = (CloudBlockBlob)Storage.GetCloudBlob(ArtifactsContainerName, ArtifactsBlobDrectory, blobName,
                        BlobType.BlockBlob);

                    if (blob.Exists())
                    {
                        Error("The block blob {0}/{1}/{2} already exists.", ArtifactsContainerName, ArtifactsBlobDrectory,
                        blobName);
                        return ApiResult.Failure;
                    }
                    else
                    {
                        if (artifact.HasData)
                        {
                            blob.UploadFromByteArray(artifact.Data, 0, artifact.Data.Length);
                        }
                        else
                        {
                            blob.UploadFromFile(artifact.Path);
                        }
                        op.Complete();
                        return ApiResult.Success;
                    }
                }
                catch (Exception e)
                {
                    Error(e, "Error occurred attempting to upload artifact {0} to container {1}",
                        artifact.Name, Profile.Name);
                    return ApiResult.Failure;
                }
            }
        }
        #endregion
    }
}
