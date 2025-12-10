using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BooksCRUD.Data.Services
{
    public class BlobService(BlobServiceClient blobServiceClient, IConfiguration configuration) : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient = blobServiceClient;
        private readonly IConfiguration _configuration = configuration;

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            string blobName = $"{Guid.NewGuid()}-{fileName}";
            string containerName = _configuration["AzureBlobStorage:UploadContainerName"];

            if (string.IsNullOrWhiteSpace(containerName))
                throw new InvalidOperationException("Upload container name missing in configuration.");

            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync();

            var blobClient = container.GetBlobClient(blobName);

            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = GetContentType(fileName)
                }
            };

            await blobClient.UploadAsync(fileStream, options);

            return blobName;
        }

        private static string GetContentType(string fileName)
        {
            return fileName.EndsWith(".png") ? "image/png" :
                   fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg") ? "image/jpeg" :
                   "application/octet-stream";
        }
        public async Task DeleteFileAsync(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
                throw new ArgumentException("Blob name must be provided.", nameof(blobName));

            string containerName = _configuration["AzureBlobStorage:UploadContainerName"];

            if (string.IsNullOrWhiteSpace(containerName))
                throw new InvalidOperationException("Upload container name missing in configuration.");

            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync();

            var blobClient = container.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }

    }
}
