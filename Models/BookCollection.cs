using System.ComponentModel.DataAnnotations;

namespace BookNest.Models
{
    public class BookCollection
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Many-to-Many relationship
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
