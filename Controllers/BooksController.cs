using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookNest.Data;
using BookNest.Models;

namespace BookNest.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(
            string? searchString,
            int? authorId,
            int? categoryId,
            bool favoritesOnly = false,
            int pageNumber = 1,
            int pageSize = 6)
        {
            // Validate page number
            if (pageNumber < 1)
                pageNumber = 1;

            var books = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
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
                books = books.Where(b => b.IsFavorite);
            }

            // Sort by title
            var sortedBooks = books.OrderBy(b => b.Title);

            // Get total count before pagination
            var totalCount = await sortedBooks.CountAsync();

            // Get total favorites count
            var favoritesCount = await _context.Books.CountAsync(b => b.IsFavorite);

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
        public async Task<IActionResult> Favorites()
        {
            var favoriteBooks = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.IsFavorite)
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
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            book.IsFavorite = !book.IsFavorite;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Books/Edit/5
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
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Title,Description,PublishedYear,ImageUrl,AuthorId,CategoryId,IsFavorite")]
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
                    _context.Update(book);
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

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
