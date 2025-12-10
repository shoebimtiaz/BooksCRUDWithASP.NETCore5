using System;

namespace BooksCRUD.Data.Utilities
{
    public static class BookImageHelper
    {
        // Base URL for CDN or public storage
        public static string BaseUrl = "https://<yourcdn>.azureedge.net/books/";

        // Generate the full URL with a version parameter
        public static string GetImageUrl(string blobName, DateTime lastUpdated)
        {
            if (string.IsNullOrEmpty(blobName)) return "/images/placeholder.png"; // fallback
                                                                                  // Use ticks as version to force browser reload when updated
            return $"{BaseUrl}{blobName}?v={lastUpdated.Ticks}";
        }
    }
}