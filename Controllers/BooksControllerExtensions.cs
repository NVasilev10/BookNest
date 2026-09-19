using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookNest.Data;
using BookNest.Models;

namespace BookNest.Controllers
{
    // Extension methods for BooksController - New Features (Author Profile & Collections)
    public partial class BooksController : Controller
    {
        // GET: Books/AuthorProfile/5
        public async Task<IActionResult> AuthorProfile(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .Include(a => a.Books)
                .ThenInclude(b => b.Reviews)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
            {
                return NotFound();
            }

            // Calculate author statistics
            var books = author.Books.ToList();
            var reviews = books.SelectMany(b => b.Reviews).ToList();

            ViewBag.Books = books;
            ViewBag.AuthorStats = new Dictionary<string, object>
            {
                { "TotalBooks", books.Count },
                { "AverageRating", reviews.Any() ? reviews.Average(r => r.Rating) : 0 },
                { "TotalReviews", reviews.Count },
                { "TotalComments", reviews.Count(r => !string.IsNullOrEmpty(r.Comment)) }
            };

            return View(author);
        }

        // GET: Books/MyCollections
        public async Task<IActionResult> MyCollections()
        {
            var collections = await _context.BookCollections
                .Include(c => c.Books)
                .ToListAsync();

            return View(collections);
        }

        // GET: Books/CollectionBooks/5
        public async Task<IActionResult> CollectionBooks(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var collection = await _context.BookCollections
                .Include(c => c.Books)
                .ThenInclude(b => b.Reviews)
                .Include(c => c.Books)
                .ThenInclude(b => b.Author)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (collection == null)
            {
                return NotFound();
            }

            return View(collection);
        }

        // GET: Books/MyRatings
        public async Task<IActionResult> MyRatings()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Book)
                .ThenInclude(b => b.Author)
                .Include(r => r.Book)
                .ThenInclude(b => b.Category)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return View(reviews);
        }

        // GET: Books/CreateCollection
        public IActionResult CreateCollection()
        {
            return View();
        }

        // POST: Books/CreateCollection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCollection([Bind("Name,Description")] BookCollection collection)
        {
            if (ModelState.IsValid)
            {
                collection.CreatedDate = DateTime.Now;
                _context.Add(collection);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyCollections));
            }
            return View(collection);
        }

        // GET: Books/EditCollection/5
        public async Task<IActionResult> EditCollection(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var collection = await _context.BookCollections.FindAsync(id);
            if (collection == null)
            {
                return NotFound();
            }

            return View(collection);
        }

        // POST: Books/UpdateCollection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCollection(int id, [Bind("Id,Name,Description")] BookCollection collection)
        {
            if (id != collection.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(collection);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookCollectionExists(collection.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(CollectionBooks), new { id = collection.Id });
            }
            return View("EditCollection", collection);
        }

        // POST: Books/AddToCollection
        [HttpPost]
        public async Task<IActionResult> AddToCollection(int bookId, int collectionId)
        {
            var book = await _context.Books.FindAsync(bookId);
            var collection = await _context.BookCollections.Include(c => c.Books).FirstOrDefaultAsync(c => c.Id == collectionId);

            if (book != null && collection != null)
            {
                if (!collection.Books.Any(b => b.Id == bookId))
                {
                    collection.Books.Add(book);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Details), new { id = bookId });
        }

        // POST: Books/RemoveFromCollection
        [HttpPost]
        public async Task<IActionResult> RemoveFromCollection(int bookId, int collectionId)
        {
            var collection = await _context.BookCollections
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.Id == collectionId);

            if (collection != null)
            {
                var book = collection.Books.FirstOrDefault(b => b.Id == bookId);
                if (book != null)
                {
                    collection.Books.Remove(book);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(CollectionBooks), new { id = collectionId });
        }

        // POST: Books/DeleteCollection
        [HttpPost]
        public async Task<IActionResult> DeleteCollection(int id)
        {
            var collection = await _context.BookCollections.FindAsync(id);
            if (collection != null)
            {
                _context.BookCollections.Remove(collection);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyCollections));
        }

        private bool BookCollectionExists(int id)
        {
            return _context.BookCollections.Any(e => e.Id == id);
        }
    }
}
