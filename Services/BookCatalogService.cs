using BookNest.Data;
using BookNest.Models;
using Microsoft.EntityFrameworkCore;

namespace BookNest.Services;

public class BookCatalogService : IBookCatalogService
{
    private readonly ApplicationDbContext _context;

    public BookCatalogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ToggleFavoriteAsync(int bookId, string userId)
    {
        if (!await _context.Books.AnyAsync(book => book.Id == bookId))
        {
            return false;
        }

        var favorite = await _context.BookFavorites.FindAsync(userId, bookId);
        if (favorite == null)
        {
            _context.BookFavorites.Add(new BookFavorite { UserId = userId, BookId = bookId });
        }
        else
        {
            _context.BookFavorites.Remove(favorite);
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
