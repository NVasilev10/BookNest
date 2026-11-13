using BookNest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookNest.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Message = TempData["Message"];
            var reviews = await _reviewService.GetAllAsync();
            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetApproval(int id, bool isApproved)
        {
            if (!await _reviewService.SetApprovalAsync(id, isApproved))
            {
                return NotFound();
            }
            TempData["Message"] = isApproved ? "Отзивът е одобрен." : "Отзивът е скрит.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _reviewService.DeleteAsync(id))
            {
                return NotFound();
            }
            TempData["Message"] = "Отзивът е изтрит.";
            return RedirectToAction(nameof(Index));
        }
    }
}
