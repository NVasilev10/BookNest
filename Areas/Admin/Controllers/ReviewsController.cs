using BookNest.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookNest.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Message = TempData["Message"];
            var reviews = await _context.Reviews
                .Include(review => review.Book)
                .Include(review => review.User)
                .OrderByDescending(review => review.CreatedDate)
                .ToListAsync();

            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetApproval(int id, bool isApproved)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            review.IsApproved = isApproved;
            await _context.SaveChangesAsync();
            TempData["Message"] = isApproved ? "Отзивът е одобрен." : "Отзивът е скрит.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Отзивът е изтрит.";
            return RedirectToAction(nameof(Index));
        }
    }
}
