using System.ComponentModel.DataAnnotations;

namespace BookNest.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заглавието е задължително.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Заглавието трябва да е между 3 и 150 знака.")]
        [Display(Name = "Заглавие")]
        public string Title { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Описанието не може да превишава 2000 знака.")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Година на издаване е задължителна.")]
        [Range(1000, 2100, ErrorMessage = "Годината на издаване трябва да е между 1000 и 2100.")]
        [Display(Name = "Година на издаване")]
        public int? PublishedYear { get; set; }

        [Url(ErrorMessage = "Невалиден URL адрес.")]
        [Display(Name = "Изображение URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Любимо")]
        public bool IsFavorite { get; set; }

        [Required(ErrorMessage = "Авторът е задължителен.")]
        [Display(Name = "Автор")]
        public int AuthorId { get; set; }
        public Author Author { get; set; } = null!;

        [Required(ErrorMessage = "Категорията е задължителна.")]
        [Display(Name = "Категория")]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
}