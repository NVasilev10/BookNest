using BookNest.Models;

namespace BookNest.Services;

public interface IReviewService
{
    Task<IReadOnlyList<Review>> GetAllAsync();
    Task<bool> CreateAsync(int bookId, string userId, Review review);
    Task<bool> SetApprovalAsync(int reviewId, bool isApproved);
    Task<bool> DeleteAsync(int reviewId);
}
