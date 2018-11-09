//---------------------------------------------------------------------------------------------------------------------------------------
// Based on Util.cs from Azure Storage Data Movement Library for .Net: https://github.com/Azure/azure-storage-net-data-movement 
//    Copyright (c) Microsoft Corporation
//---------------------------------------------------------------------------------------------------------------------------------------
namespace OLAF.Services
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;

    public class AzureStorageApi 
    {
        #region Constructors
        public AzureStorageApi(string connString, bool rethrowExceptions = false)
        {
            this.ConnectionString = connString;
            this.RethrowExceptions = rethrowExceptions;
            GetStorageAccount();
            if (this.StorageAccount != null)
            {
                this.Initialised = true;
            }
        }
        #endregion

        #region Properties
        public static int RetryTimeSeconds { get; set; } = 10;
        public static int RetryAttempts = 5;
        public string ConnectionString { get; protected set; }
        public CloudStorageAccount StorageAccount { get; protected set; }
        public CloudBlobClient BlobClient { get; protected set; }
        public bool Initialised { get; protected set; } = false;
        public bool RethrowExceptions { get; protected set; }
        #endregion

        #region Methods
        public static string GetValidAzureBlobName(string name)
        {
            StringBuilder rn = new StringBuilder(1, 1024);
            int i = 0;
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '/' || c == '_' || c =='.')
                    rn.Append(c);
                else
                    rn.Append('_');
                if (++i == 1024) break;
            }
            return rn.ToString();
        }

        public static string GetValidAzureContainerName(string name)
        {
            StringBuilder rn = new StringBuilder(1, 63);
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

        public static string GetConnectionString(Uri endPointUrl, string accountKey)
        {
            if (endPointUrl.Segments.Length < 2)
            {
                L.Error("endPointUrl {u} does not have the correct path segments", endPointUrl.ToString());
                return string.Empty;
            }
            StringBuilder csb = new StringBuilder();
            csb.AppendFormat("DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2};BlobEndpoint={3};", endPointUrl.Scheme, endPointUrl.Segments[1], accountKey, endPointUrl.Scheme + "://" + endPointUrl.Authority);
            return csb.ToString();
        }

        public async Task<ICloudBlob> GetCloudBlobAsync(string containerName, string dirName, string blobName, DateTimeOffset? snapshotTime = null)
        {
            using (var azOp = L.Begin("Get Azure Storage blob {0}/{1}/{2}", containerName, dirName, blobName))
            {
                string blobPath = "{0}/{1}".F(dirName, blobName);
                try
                {
                    GetCloudBlobClient();
                    CloudBlobContainer Container = BlobClient.GetContainerReference(containerName);
                    await Container.CreateIfNotExistsAsync();
                    ICloudBlob cloudBlob = await Container.GetBlobReferenceFromServerAsync(blobPath);
                    azOp.Complete();
                    return cloudBlob;
                }
                catch (StorageException se)
                {
                    if (RethrowExceptions)
                    {
                        throw se;
                    }
                    else
                    {
                        L.Error(se, "A storage error occurred getting Azure Storage blob {bp} in container {cn}.", blobPath, containerName);
                        return null;
                    }
                }
                catch (Exception e)
                {
                    if (RethrowExceptions)
                    {
                        throw e;
                    }
                    else
                    {
                        L.Error(e, "An error occurred getting Azure Storage blob {bp} from container {cn}.", blobPath, containerName);
                        return null;
                    }
                }
            }
        }

        public CloudBlob GetCloudBlob(string containerName, string dirName, string blobName, BlobType blobType, DateTimeOffset? snapshotTime = null)
        {
            using (var azOp = L.Begin("Get Azure Storage blob {0}/{1}/{2}", containerName, dirName, blobName))
            {
                string blobPath = "{0}/{1}".F(dirName, blobName);
                try
                {
                    GetCloudBlobClient();
                    CloudBlobContainer Container = BlobClient.GetContainerReference(containerName);
                    Container.CreateIfNotExists();
                    CloudBlob cloudBlob;
                    switch (blobType)
                    {
                        case BlobType.AppendBlob:
                            cloudBlob = Container.GetAppendBlobReference(blobPath, snapshotTime);
                            break;
                        case BlobType.BlockBlob:
                            cloudBlob = Container.GetBlockBlobReference(blobPath, snapshotTime);
                            break;
                        case BlobType.PageBlob:
                            cloudBlob = Container.GetPageBlobReference(blobPath, snapshotTime);
                            break;
                        case BlobType.Unspecified:
                        default:
                            throw new ArgumentException(string.Format("Invalid blob type {0}", blobType.ToString()), "blobType");
                    }
                    azOp.Complete();
                    return cloudBlob;
                }
                catch (StorageException se)
                {
                    if (RethrowExceptions)
                    {
                        throw se;
                    }
                    else
                    {
                        L.Error(se, "A storage error occurred getting/creating Azure Storage blob {bp} in container {cn}.", blobPath, containerName);
                        return null;
                    }
                }
                catch (Exception e)
                {
                    if (RethrowExceptions)
                    {
                        throw e;
                    }
                    else
                    {
                        L.Error(e, "An error occurred getting Azure Storage blob {bp} from container {cn}.", blobPath, containerName);
                        return null;
                    }
                }
            }

        }

        public ApiResult UploadBlobData(CloudBlockBlob blob, byte[] data)
        {
            ThrowIfNotInitialized();
            try
            {
                using (var op = L.Begin("Upload data for blob {0}.", blob.Name))
                {
                    blob.UploadFromByteArray(data, 0, data.Length);
                    string ext = blob.Name.Split('.').Last();
                    if (ext.Length != 3)
                    {
                        L.Warn("Cannot determine extension and content-type for blob {0}.", blob.Name);
                    }
                    else
                    {
                        blob.Properties.ContentType = MimeTypes.MimeTypeMap.GetMimeType(ext);
                        blob.SetProperties();
                        L.Debug("Set blob {0} content-type to {1}.", blob.Name, blob.Properties.ContentType);
                    }
                    op.Complete();
                    return ApiResult.Success;
                }
            }
            catch(Exception e)
            {
                L.Error(e, "Error occurred during upload of blob {0} data to container {1}.", blob.Name, blob.Container.Name);
                return ApiResult.Failure;
            }
        }

        public ApiResult UploadBlobData(CloudBlockBlob blob, string path)
        {
            ThrowIfNotInitialized();
            try
            {
                blob.UploadFromFile(path);
                return ApiResult.Success;
            }
            catch (Exception e)
            {
                L.Error(e, "Error occurred during upload of blob {0} data from file {1} to container {2}.", blob.Name, path, blob.Container.Name);
                return ApiResult.Failure;
            }
        }

        public ApiResult UploadBlobData(CloudBlockBlob blob, string[] text)
        {
            ThrowIfNotInitialized();
            try
            {
                blob.UploadText(string.Join(Environment.NewLine, text));
                return ApiResult.Success;
            }
            catch (Exception e)
            {
                L.Error(e, "Error occurred during upload of blob {0} text to container {1}.", blob.Name, blob.Container.Name);
                return ApiResult.Failure;
            }
        }

        /// <summary>
        /// Get a CloudBlobDirectory instance with the specified name in the given container.
        /// </summary>
        /// <param name="containerName">Container name.</param>
        /// <param name="directoryPath">Blob directory name.</param>
        /// <returns>A <see cref="Task{T}"/> object of type <see cref="CloudBlobDirectory"/> that represents the asynchronous IOperationContext.</returns>
        public CloudBlobDirectory GetCloudBlobDirectory(string containerName, string directoryPath)
        {
            using (IOperationContext azOp = L.Begin("Get Azure Storage blob directory {0)/{1}.", containerName, directoryPath))
            {
                try
                {
                    GetCloudBlobClient();
                    CloudBlobContainer container = BlobClient.GetContainerReference(containerName);
                    CloudBlobDirectory dir = container.GetDirectoryReference(directoryPath);
                    azOp.Complete();
                    return dir;
                }
                catch (StorageException se)
                {
                    if (RethrowExceptions)
                    {
                        throw se;
                    }
                    else
                    {
                        L.Error(se, "A storage error occurred getting Azure Storage directory {dn} from container {cn}.", directoryPath, containerName);
                        return null;
                    }
                }
                catch (Exception e)
                {
                    if (RethrowExceptions)
                    {
                        throw e;
                    }
                    else
                    {
                        L.Error(e, "An error occurred getting Azure Storage directory {dn} from container {cn}.", directoryPath, containerName);
                        return null;
                    }
                }
            }
        }

       
        public async Task<CloudBlobStream> OpenAppendBlobWriteStream(CloudAppendBlob blob)
        {
            BlobRequestOptions requestOptions = new BlobRequestOptions();
            OperationContext ctx = new OperationContext();
            return await blob.OpenWriteAsync(true, AccessCondition.GenerateEmptyCondition(), requestOptions, ctx, CT);
        }

        
        /// <summary>
        /// Delete the container with the specified name if it exists.
        /// </summary>
        /// <param name="containerName">Name of container to delete.</param>
        public async Task DeleteContainerAsync(string containerName)
        {
            using (IOperationContext azOp = L.Begin("Delete Azure Storage container"))
            {
                try
                {
                    CloudBlobClient client = GetCloudBlobClient();
                    CloudBlobContainer container = client.GetContainerReference(containerName);
                    await container.DeleteIfExistsAsync();
                    azOp.Complete();
                }
                catch (Exception e)
                {
                    L.Error(e, "Exception throw deleting Azure Storage container {c}.", containerName);
                }
            }
        }

        private CloudBlobClient GetCloudBlobClient()
        {
            if (BlobClient == null)
            {
                BlobClient = GetStorageAccount().CreateCloudBlobClient();
                BlobClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(RetryTimeSeconds), RetryAttempts);
            }
            return BlobClient;
        }

        private CloudStorageAccount GetStorageAccount()
        {
            try
            {
                if (StorageAccount == null)
                {
                    StorageAccount = CloudStorageAccount.Parse(ConnectionString);
                }

                return StorageAccount;
            }
            catch (Exception e)
            {
                L.Error(e, "Exception throw parsing Azure connection string {cs}.", ConnectionString);
                return null;
            }

        }

        private void ThrowIfNotInitialized()
        {
            if (!this.Initialised)
            {
                throw new InvalidOperationException("The Azure Blob Storage client is not initialized.");
            }
        }
        #endregion

        #region Fields
        static ILogger L = Global.Logger;

        CancellationToken CT { get; set; }
        #endregion
    }
}
