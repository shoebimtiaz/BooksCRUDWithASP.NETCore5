using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BooksCRUD.Web.Models
{
    public class BookCreateViewModel
    {
        [Required]
        public string Name { get; set; } = default!;

        [Required]
        public string Author { get; set; } = default!;

        [Required]
        public string Publisher { get; set; } = default!;

        public IFormFile? Image { get; set; }

        // NEW: only for Edit view (not required on Create)
        public string? ExistingImage { get; set; }
    }
}
