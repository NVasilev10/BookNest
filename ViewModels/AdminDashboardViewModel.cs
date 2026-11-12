namespace BookNest.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalBooks { get; init; }
        public int TotalAuthors { get; init; }
        public int TotalCategories { get; init; }
        public int TotalReviews { get; init; }
        public int TotalUsers { get; init; }
        public required IReadOnlyList<AdminDashboardUserViewModel> Users { get; init; }
    }

    public class AdminDashboardUserViewModel
    {
        public required string Id { get; init; }
        public string? UserName { get; init; }
        public string? Email { get; init; }
        public bool IsAdministrator { get; init; }
    }
}
