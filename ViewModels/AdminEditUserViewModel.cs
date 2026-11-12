using System.ComponentModel.DataAnnotations;

namespace BookNest.ViewModels
{
    public class AdminEditUserViewModel
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string UserName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        public string? ConfirmPassword { get; set; }
    }
}
