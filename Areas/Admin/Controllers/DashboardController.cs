using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookNest.Data;
using BookNest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookNest.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var stats = new
            {
                TotalBooks = await _context.Books.CountAsync(),
                TotalAuthors = await _context.Authors.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalReviews = await _context.Reviews.CountAsync(),
                TotalUsers = await _userManager.Users.CountAsync()
            };

            return View(stats);
        }
    }
}
