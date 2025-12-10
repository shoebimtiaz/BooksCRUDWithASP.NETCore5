using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BooksCRUD.Data.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;

        public BlobService(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
        }

        public async Task<string> UploadFileAsync(Stream file, string fileName)
        {
            var containerName = _configuration["AzureBlobStorage:UploadContainerName"];
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(fileName));

           
            await blobClient.UploadAsync(file);
            

            return blobClient.Uri.ToString();
        }
    }
}
