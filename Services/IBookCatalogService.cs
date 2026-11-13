using BookNest.Models;

namespace BookNest.Services;

public interface IBookCatalogService
{
    Task<bool> ToggleFavoriteAsync(int bookId, string userId);
}
