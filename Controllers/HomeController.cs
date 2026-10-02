using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookNest.Data;
using BookNest.Models;

namespace BookNest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.BooksCount = await _context.Books.CountAsync();

            ViewBag.FavoritesCount = await _context.Books
                .CountAsync(b => b.IsFavorite);

            ViewBag.AuthorsCount = await _context.Authors.CountAsync();

            ViewBag.CategoriesCount = await _context.Categories.CountAsync();

            // 📊 Reading Progress Statistics
            ViewBag.BooksReading = await _context.Books
                .CountAsync(b => b.Status == ReadingStatus.Reading);

            ViewBag.BooksFinished = await _context.Books
                .CountAsync(b => b.Status == ReadingStatus.Finished);

            ViewBag.BooksWantToRead = await _context.Books
                .CountAsync(b => b.Status == ReadingStatus.WantToRead);

            // Total pages read across all books
            ViewBag.TotalPagesRead = await _context.Books
                .SumAsync(b => b.PagesRead);

            // Get recent books (last 6 added)
            var recentBooks = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderByDescending(b => b.Id)
                .Take(6)
                .ToListAsync();

            ViewBag.RecentBooks = recentBooks;

            // Get favorite books
            var favoriteBooks = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.IsFavorite)
                .Take(6)
                .ToListAsync();

            ViewBag.FavoriteBooks = favoriteBooks;

            return View();
        }
    }
}