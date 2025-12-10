using System;

namespace BooksCRUD.Data.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Author { get; set; } = default!;
        public string Publisher { get; set; } = default!;

        public string? ImageBlobName { get; set; }  // Store blob URL
        public DateTime ImageUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
