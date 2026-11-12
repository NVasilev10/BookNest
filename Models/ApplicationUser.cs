using Microsoft.AspNetCore.Identity;

namespace BookNest.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Review>? Reviews { get; set; } = new List<Review>();
        public ICollection<BookCollection>? BookCollections { get; set; } = new List<BookCollection>();
    }
}
