using BookNest.Data;
using BookNest.Models;
using Microsoft.EntityFrameworkCore;

namespace BookNest.Services;

public class BookCollectionService : IBookCollectionService
{
    private readonly ApplicationDbContext _context;

    public BookCollectionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<BookCollection>> GetForUserAsync(string userId)
    {
        return await _context.BookCollections
            .Include(collection => collection.Books)
            .Where(collection => collection.UserId == userId)
            .ToListAsync();
    }

    public Task<BookCollection?> GetOwnedWithBooksAsync(int collectionId, string userId)
    {
        return _context.BookCollections
            .Include(collection => collection.Books)
                .ThenInclude(book => book.Reviews)
            .Include(collection => collection.Books)
                .ThenInclude(book => book.Author)
            .FirstOrDefaultAsync(collection => collection.Id == collectionId && collection.UserId == userId);
    }

    public Task<BookCollection?> GetOwnedAsync(int collectionId, string userId)
    {
        return _context.BookCollections
            .FirstOrDefaultAsync(collection => collection.Id == collectionId && collection.UserId == userId);
    }

    public async Task CreateAsync(BookCollection collection, string userId)
    {
        collection.UserId = userId;
        collection.CreatedDate = DateTime.Now;
        _context.BookCollections.Add(collection);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(int collectionId, string userId, string name, string? description)
    {
        var collection = await GetOwnedAsync(collectionId, userId);
        if (collection == null)
        {
            return false;
        }

        collection.Name = name;
        collection.Description = description;

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.BookCollections.AnyAsync(item => item.Id == collectionId))
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> AddBookAsync(int bookId, int collectionId, string userId)
    {
        var book = await _context.Books.FindAsync(bookId);
        var collection = await _context.BookCollections
            .Include(item => item.Books)
            .FirstOrDefaultAsync(item => item.Id == collectionId && item.UserId == userId);

        if (book == null || collection == null)
        {
            return false;
        }

        if (collection.Books.All(item => item.Id != bookId))
        {
            collection.Books.Add(book);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> RemoveBookAsync(int bookId, int collectionId, string userId)
    {
        var collection = await _context.BookCollections
            .Include(item => item.Books)
            .FirstOrDefaultAsync(item => item.Id == collectionId && item.UserId == userId);

        if (collection == null)
        {
            return false;
        }

        var book = collection.Books.FirstOrDefault(item => item.Id == bookId);
        if (book != null)
        {
            collection.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> DeleteAsync(int collectionId, string userId)
    {
        var collection = await GetOwnedAsync(collectionId, userId);
        if (collection == null)
        {
            return false;
        }

        _context.BookCollections.Remove(collection);
        await _context.SaveChangesAsync();
        return true;
    }
}
