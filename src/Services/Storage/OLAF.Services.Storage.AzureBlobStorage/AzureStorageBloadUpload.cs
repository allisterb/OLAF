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
            ArtifactsBlobDirectory = AzureStorageApi.GetValidAzureBlobName(Profile.ArtifactsDirectory.Name);
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
                return UploadArtifact(artifact);
            }
            else
            {
                return UploadArtifact(artifact);
            }
           
        }
        #endregion

        #region Properties
        public static string ArtifactsContainerName { get; } = AzureStorageApi.GetValidAzureContainerName("olafartifacts");

        public string ArtifactsBlobDirectory { get; }

        public AzureStorageApi Storage { get; protected set; }

        protected bool UseEmulator { get; }
        #endregion

        #region Methods
        protected ApiResult UploadArtifact(Artifact artifact)
        {
            CloudBlockBlob blob = null;
            string blobName = AzureStorageApi.GetValidAzureBlobName(artifact.Name);
            string blobPath = GetBlobPathForArtifact(artifact);
            using (var op = Begin("Uploading blob {0} to Azure Blob Storage.", ArtifactsContainerName, blobPath))
            {
                blob = (CloudBlockBlob)Storage.GetCloudBlob(ArtifactsContainerName, ArtifactsBlobDirectory, blobName,
                    BlobType.BlockBlob);
                if (blob == null)
                {
                    Error("Could not get reference to blob {0}.", blobPath);
                    return ApiResult.Failure;
                }
                else if (blob.Exists())
                {
                    Error("The block blob {0} already exists.", blobPath);
                    return ApiResult.Failure;
                }
                else
                {
                    if (artifact is FileArtifact fileArtifact)
                    {
                        if (fileArtifact.HasData)
                        {
                            return Storage.UploadBlobData(blob, fileArtifact.Data);
                        }
                        else
                        {
                            return Storage.UploadBlobData(blob, fileArtifact.Path);
                        }
                    }
                    else if (artifact is ImageArtifact imageArtifact)
                    {
                        if (imageArtifact.HasData)
                        {
                            return Storage.UploadBlobData(blob, imageArtifact.Data);
                        }
                        else if (imageArtifact.HasFile && imageArtifact.FileArtifact.HasData)
                        {
                            return Storage.UploadBlobData(blob, imageArtifact.FileArtifact.Data);
                        }
                        else if (imageArtifact.HasFile)
                        {
                            return Storage.UploadBlobData(blob, imageArtifact.FileArtifact.Path);
                        }
                        else throw new Exception("Image artifact {0} does not have associated data or file.".F(artifact.Id));
                    }
                    else
                        throw new NotImplementedException("Unknown artifact type.");  
                }
                
            }
        }

        protected string GetBlobPathForArtifact(Artifact artifact) => "{0}/{1}/{2}".F(ArtifactsContainerName, ArtifactsBlobDirectory,
            AzureStorageApi.GetValidAzureBlobName(artifact.Name));
        #endregion
    }
}
