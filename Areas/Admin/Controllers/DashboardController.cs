using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookNest.Data;
using BookNest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BookNest.ViewModels;

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
            var users = await _userManager.Users
                .OrderBy(user => user.UserName)
                .ToListAsync();
            var administratorIds = (await _userManager.GetUsersInRoleAsync("Administrator"))
                .Select(user => user.Id)
                .ToHashSet();

            var model = new AdminDashboardViewModel
            {
                TotalBooks = await _context.Books.CountAsync(),
                TotalAuthors = await _context.Authors.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalReviews = await _context.Reviews.CountAsync(),
                TotalUsers = users.Count,
                Users = users.Select(user => new AdminDashboardUserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    IsAdministrator = administratorIds.Contains(user.Id)
                }).ToList()
            };

            ViewBag.Message = TempData["Message"];
            ViewBag.Error = TempData["Error"];
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAdministratorRole(string userId, bool grantAdmin)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (!grantAdmin && user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "Не можете да премахнете администраторската роля от собствения си акаунт.";
                return RedirectToAction(nameof(Index));
            }

            if (!grantAdmin)
            {
                var administrators = await _userManager.GetUsersInRoleAsync("Administrator");
                if (administrators.Count <= 1)
                {
                    TempData["Error"] = "Не може да бъде премахната ролята на последния администратор.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var result = grantAdmin
                ? await _userManager.AddToRoleAsync(user, "Administrator")
                : await _userManager.RemoveFromRoleAsync(user, "Administrator");

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(" ", result.Errors.Select(error => error.Description));
            }
            else
            {
                TempData["Message"] = grantAdmin
                    ? $"На {user.UserName} е дадена администраторска роля."
                    : $"Администраторската роля на {user.UserName} е премахната.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
