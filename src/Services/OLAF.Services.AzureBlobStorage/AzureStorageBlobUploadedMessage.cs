using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace OLAF.Services
{
    public class AzureStorageBlobUploadedMessage : Message
    {
        public AzureStorageBlobUploadedMessage(long id, CloudBlockBlob blob) :base(id)
        {
            Blob = blob;
        }

        public CloudBlockBlob Blob { get; }
    }
}
