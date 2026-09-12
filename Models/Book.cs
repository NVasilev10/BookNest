using System.ComponentModel.DataAnnotations;

namespace BookNest.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заглавието е задължително.")]
        [StringLength(150)]
        public string Title { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(1000, 2100)]
        public int PublishedYear { get; set; }

        [Url]
        public string? ImageUrl { get; set; }

        // ⭐ Favorite
        public bool IsFavorite { get; set; }

        // Author relationship
        public int AuthorId { get; set; }
        public Author Author { get; set; } = null!;

        // Category relationship
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
}