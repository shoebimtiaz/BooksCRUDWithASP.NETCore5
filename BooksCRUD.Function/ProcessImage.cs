using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using BooksCRUD.Image;

namespace BooksCRUD.Function
{
    public class ProcessBookImage(ILogger<ProcessBookImage> logger, BlobServiceClient blobService)
    {
        private readonly ILogger<ProcessBookImage> _logger = logger;
        private readonly BlobServiceClient _blobService = blobService;
        private readonly ImageProcessor _processor = new ImageProcessor();

        [Function("ProcessBookImage")]
        public async Task Run([QueueTrigger("image-processing", Connection = "AzureWebJobsStorage")] string queueMessage)
        {
            var msg = JsonSerializer.Deserialize<BookImageMessage>(queueMessage);
            if (msg == null) return;

            var container = _blobService.GetBlobContainerClient("books-images");
            var blob = container.GetBlobClient($"original/{msg.BlobName}");
            if (!await blob.ExistsAsync()) return;

            using var stream = new MemoryStream();
            await blob.DownloadToAsync(stream);
            stream.Position = 0;

            await _processor.ProcessImage(stream, container, msg.BlobName);
            _logger.LogInformation("Processed image {BlobName}", msg.BlobName);
        }
    }

    public record BookImageMessage(string BlobName);
}
