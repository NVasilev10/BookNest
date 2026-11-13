using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using BookNest.Data;
using BookNest.Models;
using BookNest.Services;

namespace BookNest.Controllers
{
    public partial class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookCatalogService _bookCatalogService;
        private readonly IBookCollectionService _bookCollectionService;
        private readonly IReviewService _reviewService;

        public BooksController(
            ApplicationDbContext context,
            IBookCatalogService bookCatalogService,
            IBookCollectionService bookCollectionService,
            IReviewService reviewService)
        {
            _context = context;
            _bookCatalogService = bookCatalogService;
            _bookCollectionService = bookCollectionService;
            _reviewService = reviewService;
        }

        // GET: Books
        public async Task<IActionResult> Index(
            string? searchString,
            int? authorId,
            int? categoryId,
            bool favoritesOnly = false,
            string sortBy = "title",
            int pageNumber = 1,
            int pageSize = 6)
        {
            // Validate page number
            if (pageNumber < 1)
                pageNumber = 1;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favoriteBookIds = _context.BookFavorites.Where(f => f.UserId == userId).Select(f => f.BookId);
            var books = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .AsQueryable();

            // 🔍 Advanced Search - search by title, description, and author name
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lowerSearch = searchString.ToLower();
                books = books.Where(b => 
                    b.Title.ToLower().Contains(lowerSearch) ||
                    b.Description.ToLower().Contains(lowerSearch) ||
                    b.Author.Name.ToLower().Contains(lowerSearch)
                );
            }

            // ✍️ Filter by Author
            if (authorId.HasValue)
            {
                books = books.Where(b => b.AuthorId == authorId.Value);
            }

            // 🏷️ Filter by Category
            if (categoryId.HasValue)
            {
                books = books.Where(b => b.CategoryId == categoryId.Value);
            }

            // ⭐ Favorites only
            if (favoritesOnly)
            {
                books = books.Where(b => favoriteBookIds.Contains(b.Id));
            }

            // 📊 Apply sorting
            IQueryable<Book> sortedBooks = sortBy?.ToLower() switch
            {
                "rating_high" => books.OrderByDescending(b => 
                    b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0),
                "rating_low" => books.OrderBy(b => 
                    b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0),
                "newest" => books.OrderByDescending(b => b.PublishedYear),
                "oldest" => books.OrderBy(b => b.PublishedYear),
                _ => books.OrderBy(b => b.Title) // Default: sort by title
            };

            // Get total count before pagination
            var totalCount = await sortedBooks.CountAsync();

            // Get total favorites count
            var favoritesCount = await favoriteBookIds.CountAsync();
            ViewData["FavoriteBookIds"] = (await favoriteBookIds.ToListAsync()).ToHashSet();

            // Apply pagination
            var paginatedBooks = await sortedBooks
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calculate total pages
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Dropdown Authors
            ViewData["Authors"] = new SelectList(
                await _context.Authors.ToListAsync(),
                "Id",
                "Name",
                authorId
            );

            // Dropdown Categories
            ViewData["Categories"] = new SelectList(
                await _context.Categories.ToListAsync(),
                "Id",
                "Name",
                categoryId
            );

            ViewData["SearchString"] = searchString;
            ViewData["FavoritesOnly"] = favoritesOnly;
            ViewData["SortBy"] = sortBy;
            ViewData["ResultCount"] = totalCount;
            ViewData["FavoritesCount"] = favoritesCount;
            ViewData["PageNumber"] = pageNumber;
            ViewData["TotalPages"] = totalPages;
            ViewData["PageSize"] = pageSize;
            ViewData["HasPreviousPage"] = pageNumber > 1;
            ViewData["HasNextPage"] = pageNumber < totalPages;

            return View(paginatedBooks);
        }

        // GET: Books/Favorites
        [Authorize]
        public async Task<IActionResult> Favorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favoriteBooks = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => _context.BookFavorites.Any(f => f.UserId == userId && f.BookId == b.Id))
                .OrderBy(b => b.Title)
                .ToListAsync();

            ViewData["FavoritesCount"] = favoriteBooks.Count;
            return View(favoriteBooks);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Reviews.OrderByDescending(r => r.CreatedDate))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            // Calculate average rating
            var averageRating = book.Reviews.Any() 
                ? book.Reviews.Average(r => r.Rating) 
                : 0;
            ViewData["AverageRating"] = averageRating;
            ViewData["ReviewCount"] = book.Reviews.Count;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var collections = userId == null
                ? new List<BookCollection>()
                : await _bookCollectionService.GetForUserAsync(userId);
            ViewData["Collections"] = collections;
            ViewData["IsFavorite"] = userId != null && await _context.BookFavorites.AnyAsync(f => f.UserId == userId && f.BookId == book.Id);

            return View(book);
        }

        // GET: Books/Create
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        private async Task PopulateDropdowns(int selectedAuthorId = 0, int selectedCategoryId = 0)
        {
            var authors = await _context.Authors.ToListAsync();
            var categories = await _context.Categories.ToListAsync();

            if (!authors.Any())
            {
                ModelState.AddModelError("", "No authors available. Please add an author first.");
            }
            if (!categories.Any())
            {
                ModelState.AddModelError("", "No categories available. Please add a category first.");
            }

            ViewBag.AuthorId = new SelectList(authors, "Id", "Name", selectedAuthorId);
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", selectedCategoryId);
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(Book book)
        {
            // Remove IsFavorite and Author/Category navigation properties from ModelState
            ModelState.Remove("IsFavorite");
            ModelState.Remove("Author");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(book);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error saving book: {ex.Message}");
                }
            }

            // Populate dropdowns again for display
            await PopulateDropdowns(book.AuthorId, book.CategoryId);

            return View(book);
        }

        // ⭐ Toggle Favorite
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Challenge();
            if (!await _bookCatalogService.ToggleFavoriteAsync(id, userId))
                return NotFound();

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Books/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            ViewBag.AuthorId = new SelectList(
                _context.Authors,
                "Id",
                "Name",
                book.AuthorId
            );

            ViewBag.CategoryId = new SelectList(
                _context.Categories,
                "Id",
                "Name",
                book.CategoryId
            );

            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Title,Description,PublishedYear,ImageUrl,AuthorId,CategoryId,Status,TotalPages,PagesRead,StartDate,FinishDate")]
            Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            // Remove navigation properties from ModelState
            ModelState.Remove("Author");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBook = await _context.Books.FindAsync(id);
                    if (existingBook == null)
                        return NotFound();
                    _context.Entry(existingBook).CurrentValues.SetValues(book);
                    _context.Entry(existingBook).Property(b => b.IsFavorite).IsModified = false;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error saving book: {ex.Message}");
                }
            }

            // Populate dropdowns again for display
            await PopulateDropdowns(book.AuthorId, book.CategoryId);
            return View(book);
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Books/CreateReview
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview(int bookId, [Bind("Rating,Comment")] Review review)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Challenge();
            if (!await _context.Books.AnyAsync(b => b.Id == bookId))
                return NotFound();
            review.BookId = bookId;
            review.UserId = userId;
            ModelState.Remove(nameof(Review.Book));

            if (!ModelState.IsValid)
            {
                TempData["ReviewError"] = "Please select a rating from 1 to 5 and use a comment of at most 500 characters.";
                return RedirectToAction(nameof(Details), new { id = bookId });
            }

            if (!await _reviewService.CreateAsync(bookId, userId, review))
                return NotFound();

            return RedirectToAction(nameof(Details), new { id = bookId });
        }

        // GET: Books/Statistics
        public async Task<IActionResult> Statistics()
        {
            // Total counts
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var totalBooks = await _context.Books.CountAsync();
            var totalAuthors = await _context.Authors.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalFavorites = await _context.BookFavorites.CountAsync(f => f.UserId == userId);
            var totalReviews = await _context.Reviews.CountAsync();

            // Popular authors (by book count)
            var popularAuthors = await _context.Authors
                .Include(a => a.Books)
                .OrderByDescending(a => a.Books.Count)
                .Take(5)
                .ToListAsync();

            // Popular categories (by book count)
            var popularCategories = await _context.Categories
                .Include(c => c.Books)
                .OrderByDescending(c => c.Books.Count)
                .Take(5)
                .ToListAsync();

            // Top rated books
            var topRatedBooks = await _context.Books
                .Include(b => b.Reviews)
                .Include(b => b.Author)
                .Where(b => b.Reviews.Any())
                .OrderByDescending(b => b.Reviews.Average(r => r.Rating))
                .Take(5)
                .ToListAsync();

            // Average rating across all books
            var averageRating = await _context.Reviews.AnyAsync() 
                ? _context.Reviews.Average(r => r.Rating) 
                : 0;

            ViewData["TotalBooks"] = totalBooks;
            ViewData["TotalAuthors"] = totalAuthors;
            ViewData["TotalCategories"] = totalCategories;
            ViewData["TotalFavorites"] = totalFavorites;
            ViewData["TotalReviews"] = totalReviews;
            ViewData["AverageRating"] = averageRating;
            ViewData["PopularAuthors"] = popularAuthors;
            ViewData["PopularCategories"] = popularCategories;
            ViewData["TopRatedBooks"] = topRatedBooks;

            return View();
        }

        // GET: Books/ExportAsCsv
        public async Task<IActionResult> ExportAsCsv(
            string? searchString,
            int? authorId,
            int? categoryId,
            bool favoritesOnly = false,
            string sortBy = "title")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favoriteBookIds = _context.BookFavorites.Where(f => f.UserId == userId).Select(f => f.BookId);
            var books = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .AsQueryable();

            // Apply same filters as Index
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lowerSearch = searchString.ToLower();
                books = books.Where(b =>
                    b.Title.ToLower().Contains(lowerSearch) ||
                    b.Description.ToLower().Contains(lowerSearch) ||
                    b.Author.Name.ToLower().Contains(lowerSearch)
                );
            }

            if (authorId.HasValue)
            {
                books = books.Where(b => b.AuthorId == authorId.Value);
            }

            if (categoryId.HasValue)
            {
                books = books.Where(b => b.CategoryId == categoryId.Value);
            }

            if (favoritesOnly)
            {
                books = books.Where(b => favoriteBookIds.Contains(b.Id));
            }

            var booksToExport = await books.ToListAsync();
            var favorites = (await favoriteBookIds.ToListAsync()).ToHashSet();

            // Generate CSV
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Title,Author,Category,Published Year,Is Favorite,Average Rating");

            foreach (var book in booksToExport)
            {
                var avgRating = book.Reviews.Any() ? book.Reviews.Average(r => r.Rating) : 0;
                var line = $"\"{book.Title}\",\"{book.Author?.Name ?? "N/A"}\",\"{book.Category?.Name ?? "N/A"}\",{book.PublishedYear},{favorites.Contains(book.Id)},{avgRating}";
                csv.AppendLine(line);
            }

            var fileName = $"books_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        // GET: Books/ExportAsJson
        public async Task<IActionResult> ExportAsJson(
            string? searchString,
            int? authorId,
            int? categoryId,
            bool favoritesOnly = false,
            string sortBy = "title")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favoriteBookIds = _context.BookFavorites.Where(f => f.UserId == userId).Select(f => f.BookId);
            var books = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .AsQueryable();

            // Apply same filters as Index
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lowerSearch = searchString.ToLower();
                books = books.Where(b =>
                    b.Title.ToLower().Contains(lowerSearch) ||
                    b.Description.ToLower().Contains(lowerSearch) ||
                    b.Author.Name.ToLower().Contains(lowerSearch)
                );
            }

            if (authorId.HasValue)
            {
                books = books.Where(b => b.AuthorId == authorId.Value);
            }

            if (categoryId.HasValue)
            {
                books = books.Where(b => b.CategoryId == categoryId.Value);
            }

            if (favoritesOnly)
            {
                books = books.Where(b => favoriteBookIds.Contains(b.Id));
            }

            var booksToExport = await books.ToListAsync();
            var favorites = (await favoriteBookIds.ToListAsync()).ToHashSet();

            // Transform to export-friendly format
            var exportData = booksToExport.Select(b => new
            {
                b.Id,
                b.Title,
                Author = b.Author?.Name ?? "N/A",
                Category = b.Category?.Name ?? "N/A",
                b.PublishedYear,
                IsFavorite = favorites.Contains(b.Id),
                AverageRating = b.Reviews.Any() ? Math.Round(b.Reviews.Average(r => r.Rating), 2) : 0,
                ReviewCount = b.Reviews.Count
            }).ToList();

            var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            var fileName = $"books_export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
        }
    }
}
