using BookNest.Data;
using BookNest.Models;
using Microsoft.EntityFrameworkCore;

namespace BookNest.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;

    public ReviewService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Review>> GetAllAsync()
    {
        return await _context.Reviews
            .Include(review => review.Book)
            .Include(review => review.User)
            .OrderByDescending(review => review.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> CreateAsync(int bookId, string userId, Review review)
    {
        if (!await _context.Books.AnyAsync(book => book.Id == bookId))
        {
            return false;
        }

        review.BookId = bookId;
        review.UserId = userId;
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetApprovalAsync(int reviewId, bool isApproved)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null)
        {
            return false;
        }

        review.IsApproved = isApproved;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null)
        {
            return false;
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }
}
