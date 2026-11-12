using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookNest.Models;
using BookNest.ViewModels;
using System.Security.Claims;

namespace BookNest.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;
            var totalCount = await _userManager.Users.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            page = Math.Clamp(page, 1, Math.Max(totalPages, 1));

            var users = await _userManager.Users
                .OrderBy(user => user.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Message = TempData["Message"];
            ViewBag.Error = TempData["Error"];

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                var deleteResult = await _userManager.DeleteAsync(user);
                var errors = roleResult.Errors.Select(error => error.Description);
                if (!deleteResult.Succeeded)
                {
                    errors = errors.Concat(deleteResult.Errors.Select(error => error.Description));
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return View(model);
            }

            TempData["Message"] = $"Потребителят {user.UserName} е създаден.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(new AdminEditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminEditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(model.NewPassword) && model.NewPassword.Length < 6)
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Паролата трябва да е поне 6 символа.");
            }

            var existingUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserName != null && existingUserName.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.UserName), "Това потребителско име вече е заето.");
            }

            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null && existingEmail.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "Този имейл вече се използва.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            user.UserName = model.UserName;
            user.NormalizedUserName = _userManager.NormalizeName(model.UserName);
            user.Email = model.Email;
            user.NormalizedEmail = _userManager.NormalizeEmail(model.Email);
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError(nameof(model.NewPassword), error.Description);
                    }

                    return View(model);
                }
            }

            TempData["Message"] = $"Данните на {user.UserName} са обновени.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ManageRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles
                .Where(role => role.Name == "User" || role.Name == "Administrator")
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.AllRoles = allRoles;
            ViewBag.UserRoles = userRoles;
            ViewBag.Error = TempData["Error"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(string userId, string roleName, bool addRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();
            if (roleName != "User" && roleName != "Administrator")
                return BadRequest();

            if (!addRole && roleName == "Administrator")
            {
                if (user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier))
                {
                    TempData["Error"] = "Не можете да премахнете администраторската роля от собствения си акаунт.";
                    return RedirectToAction(nameof(ManageRoles), new { id = userId });
                }

                var administrators = await _userManager.GetUsersInRoleAsync("Administrator");
                if (administrators.Count <= 1)
                {
                    TempData["Error"] = "Не може да бъде премахната ролята на последния администратор.";
                    return RedirectToAction(nameof(ManageRoles), new { id = userId });
                }
            }

            var result = addRole
                ? await _userManager.AddToRoleAsync(user, roleName)
                : await _userManager.RemoveFromRoleAsync(user, roleName);

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(" ", result.Errors.Select(error => error.Description));
            }

            return RedirectToAction(nameof(ManageRoles), new { id = userId });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "Не можете да изтриете акаунта, с който сте влезли.";
                return RedirectToAction(nameof(Index));
            }

            if (await _userManager.IsInRoleAsync(user, "Administrator"))
            {
                var administrators = await _userManager.GetUsersInRoleAsync("Administrator");
                if (administrators.Count <= 1)
                {
                    TempData["Error"] = "Не може да бъде изтрит последният администратор.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(" ", result.Errors.Select(error => error.Description));
            }
            else
            {
                TempData["Message"] = $"Потребителят {user.UserName} е изтрит.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
