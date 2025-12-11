using System;

namespace BooksCRUD.Data.Models
{
    public class BookLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string BookId { get; set; } = default!;
        public string Action { get; set; } = default!;  // e.g., "Create", "Edit", "Delete"
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }  // optional info like "Book title was updated"
    }
}
