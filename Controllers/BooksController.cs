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
            bool favoritesOnly = false)
        {
            var books = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .AsQueryable();

            // 🔍 Search by title
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString));
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

            return View(await books.ToListAsync());
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
