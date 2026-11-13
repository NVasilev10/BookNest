using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using BookNest.Data;
using BookNest.Models;
using BookNest.Services;

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
        [Authorize]
        public async Task<IActionResult> MyCollections()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Challenge();

            var collections = await _bookCollectionService.GetForUserAsync(userId);
            return View(collections);
        }

        // GET: Books/CollectionBooks/5
        [Authorize]
        public async Task<IActionResult> CollectionBooks(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return NotFound();

            var collection = await _bookCollectionService.GetOwnedWithBooksAsync(id.Value, userId);
            if (collection == null)
            {
                return NotFound();
            }

            return View(collection);
        }

        // GET: Books/MyRatings
        [Authorize]
        public async Task<IActionResult> MyRatings(string sortBy = "recent")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reviews = await _context.Reviews
                .Include(r => r.Book)
                .ThenInclude(b => b.Author)
                .Include(r => r.Book)
                .ThenInclude(b => b.Category)
                .Where(r => userId != null && r.UserId == userId)
                .ToListAsync();

            // Build view models
            var viewModels = new List<MyRatingsViewModel>();
            var groupedByBook = reviews.GroupBy(r => r.BookId);

            foreach (var bookReviews in groupedByBook)
            {
                var myReview = bookReviews.OrderByDescending(r => r.CreatedDate).First();
                var allReviews = await _context.Reviews
                    .Where(r => r.BookId == bookReviews.Key)
                    .ToListAsync();

                var viewModel = new MyRatingsViewModel
                {
                    Book = myReview.Book,
                    MyRating = myReview.Rating,
                    MyComment = myReview.Comment,
                    MyRatingDate = myReview.CreatedDate,
                    AverageRating = allReviews.Any() ? allReviews.Average(r => r.Rating) : 0,
                    TotalReviews = allReviews.Count
                };
                viewModels.Add(viewModel);
            }

            // Sort results
            viewModels = sortBy switch
            {
                "oldest" => viewModels.OrderBy(v => v.MyRatingDate).ToList(),
                "highest" => viewModels.OrderByDescending(v => v.MyRating).ToList(),
                "lowest" => viewModels.OrderBy(v => v.MyRating).ToList(),
                _ => viewModels.OrderByDescending(v => v.MyRatingDate).ToList()
            };

            ViewData["SortBy"] = sortBy;
            ViewData["RatingCount"] = viewModels.Count;

            return View(viewModels);
        }

        // GET: Books/CreateCollection
        [Authorize]
        public IActionResult CreateCollection()
        {
            return View();
        }

        // POST: Books/CreateCollection
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCollection([Bind("Name,Description")] BookCollection collection)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Challenge();
            if (ModelState.IsValid)
            {
                await _bookCollectionService.CreateAsync(collection, userId);
                return RedirectToAction(nameof(MyCollections));
            }
            return View(collection);
        }

        // GET: Books/EditCollection/5
        [Authorize]
        public async Task<IActionResult> EditCollection(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return NotFound();

            var collection = await _bookCollectionService.GetOwnedAsync(id.Value, userId);
            if (collection == null)
            {
                return NotFound();
            }

            return View(collection);
        }

        // POST: Books/UpdateCollection
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCollection(int id, [Bind("Id,Name,Description")] BookCollection collection)
        {
            if (id != collection.Id)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return NotFound();

            if (!ModelState.IsValid && await _bookCollectionService.GetOwnedAsync(id, userId) == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                if (!await _bookCollectionService.UpdateAsync(id, userId, collection.Name, collection.Description))
                    return NotFound();

                return RedirectToAction(nameof(CollectionBooks), new { id = collection.Id });
            }
            return View("EditCollection", collection);
        }

        // POST: Books/AddToCollection
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCollection(int bookId, int collectionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !await _bookCollectionService.AddBookAsync(bookId, collectionId, userId))
                return NotFound();

            return RedirectToAction(nameof(Details), new { id = bookId });
        }

        // POST: Books/RemoveFromCollection
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCollection(int bookId, int collectionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !await _bookCollectionService.RemoveBookAsync(bookId, collectionId, userId))
                return NotFound();

            return RedirectToAction(nameof(CollectionBooks), new { id = collectionId });
        }

        // POST: Books/DeleteCollection
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCollection(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !await _bookCollectionService.DeleteAsync(id, userId))
                return NotFound();

            return RedirectToAction(nameof(MyCollections));
        }

    }
}
