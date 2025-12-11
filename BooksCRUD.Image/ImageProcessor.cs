using Azure.Storage.Blobs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;

namespace BooksCRUD.Image
{
    public class ImageProcessor
    {
        public async Task ProcessImage(Stream inputStream, BlobContainerClient container, string blobName)
        {
            using var image = SixLabors.ImageSharp.Image.Load(inputStream);

            await SaveImage(container, $"original/{blobName}", image);
            await SaveImage(container, $"full/{blobName}", image, 1200);
            await SaveImage(container, $"medium/{blobName}", image, 600);
            await SaveImage(container, $"thumbs/{blobName}", image, 150);
        }

        private static async Task SaveImage(BlobContainerClient container, string blobPath, SixLabors.ImageSharp.Image image, int? width = null)
        {
            var img = width.HasValue
                ? image.Clone(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(width.Value, 0) }))
                : image;

            using var ms = new MemoryStream();
            img.Save(ms, new PngEncoder());
            ms.Position = 0;

            await container.UploadBlobAsync(blobPath, ms);
        }
    }
}
