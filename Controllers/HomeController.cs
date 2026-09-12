using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookNest.Data;

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

            return View();
        }
    }
}