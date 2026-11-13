using BookNest.Models;

namespace BookNest.Services;

public interface IBookCollectionService
{
    Task<IReadOnlyList<BookCollection>> GetForUserAsync(string userId);
    Task<BookCollection?> GetOwnedWithBooksAsync(int collectionId, string userId);
    Task<BookCollection?> GetOwnedAsync(int collectionId, string userId);
    Task CreateAsync(BookCollection collection, string userId);
    Task<bool> UpdateAsync(int collectionId, string userId, string name, string? description);
    Task<bool> AddBookAsync(int bookId, int collectionId, string userId);
    Task<bool> RemoveBookAsync(int bookId, int collectionId, string userId);
    Task<bool> DeleteAsync(int collectionId, string userId);
}
