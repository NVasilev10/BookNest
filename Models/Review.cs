using System.ComponentModel.DataAnnotations;

namespace BookNest.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Оценката е задължителна.")]
        [Range(1, 5, ErrorMessage = "Оценката трябва да е между 1 и 5 звезди.")]
        [Display(Name = "Оценка")]
        public int Rating { get; set; } // 1-5 stars

        [StringLength(500, ErrorMessage = "Коментарът не може да превишава 500 символа.")]
        [Display(Name = "Коментар")]
        public string? Comment { get; set; }

        [Display(Name = "Дата на рецензия")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Foreign Key
        [Required]
        [Display(Name = "Книга")]
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;
    }
}
