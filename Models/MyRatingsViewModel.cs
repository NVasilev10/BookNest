namespace BookNest.Models
{
    public class MyRatingsViewModel
    {
        public Book Book { get; set; }
        public int MyRating { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public string MyComment { get; set; }
        public DateTime MyRatingDate { get; set; }
    }
}
