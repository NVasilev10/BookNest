using System.ComponentModel.DataAnnotations;

namespace BookNest.Models
{
    public enum ReadingStatus
    {
        [Display(Name = "Искам да чета")]
        WantToRead = 0,

        [Display(Name = "Чета")]
        Reading = 1,

        [Display(Name = "Прочетена")]
        Finished = 2
    }

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

        // Reading Progress Tracking
        [Display(Name = "Статус")]
        public ReadingStatus Status { get; set; } = ReadingStatus.WantToRead;

        [Display(Name = "Общо страни")]
        [Range(1, 10000, ErrorMessage = "Броят на страниците трябва да е между 1 и 10000.")]
        public int? TotalPages { get; set; }

        [Display(Name = "Прочетени страни")]
        [Range(0, 10000, ErrorMessage = "Броят на прочетените страни трябва да е между 0 и 10000.")]
        public int PagesRead { get; set; } = 0;

        [Display(Name = "Дата на начало")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Дата на завършване")]
        public DateTime? FinishDate { get; set; }

        // Reviews relationship
        public List<Review> Reviews { get; set; } = new();

        // Collections relationship
        public ICollection<BookCollection> Collections { get; set; } = new List<BookCollection>();

        // Helper property to calculate progress percentage
        public int ProgressPercentage
        {
            get
            {
                if (TotalPages == null || TotalPages == 0)
                    return 0;
                return (int)((PagesRead / (double)TotalPages) * 100);
            }
        }
    }
}