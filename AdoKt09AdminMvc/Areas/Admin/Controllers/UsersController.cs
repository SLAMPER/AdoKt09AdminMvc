using AdoKt09AdminMvc.Infrastructure.Auth;
using AdoKt09AdminMvc.Models.Identity;
using AdoKt09AdminMvc.ViewModels.Admin.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdoKt09AdminMvc.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class UsersController(UserManager<AppUser> userManager) : Controller
{
	private readonly UserManager<AppUser> _userManager = userManager;

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();
		var model = new List<UserListItemViewModel>(users.Count);

		foreach (var user in users)
		{
			model.Add(new UserListItemViewModel
			{
				Id = user.Id,
				UserName = user.UserName ?? string.Empty,
				Email = user.Email ?? string.Empty,
				IsAdmin = await _userManager.IsInRoleAsync(user, RoleNames.Administrator)
			});
		}

		return View(model);
	}

	[HttpGet]
	public IActionResult Create()
	{
		return View(new CreateUserViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(CreateUserViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View(model);
		}

		var user = new AppUser
		{
			UserName = model.UserName,
			Email = model.Email,
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

		if (model.IsAdmin)
		{
			var addToRoleResult = await _userManager.AddToRoleAsync(user, RoleNames.Administrator);
			if (!addToRoleResult.Succeeded)
			{
				foreach (var error in addToRoleResult.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}

				return View(model);
			}
		}
		else
		{
			await _userManager.AddToRoleAsync(user, RoleNames.User);
		}

		return RedirectToAction(nameof(Index));
	}

	[HttpGet]
	public async Task<IActionResult> Edit(string id)
	{
		var user = await _userManager.FindByIdAsync(id);
		if (user is null)
		{
			return NotFound();
		}

		var model = new EditUserViewModel
		{
			Id = user.Id,
			UserName = user.UserName ?? string.Empty,
			Email = user.Email ?? string.Empty,
			IsAdmin = await _userManager.IsInRoleAsync(user, RoleNames.Administrator)
		};

		return View(model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(string id, EditUserViewModel model)
	{
		if (id != model.Id)
		{
			return BadRequest();
		}

		if (!ModelState.IsValid)
		{
			return View(model);
		}

		var user = await _userManager.FindByIdAsync(id);
		if (user is null)
		{
			return NotFound();
		}

		user.UserName = model.UserName;
		user.Email = model.Email;
		user.NormalizedEmail = _userManager.NormalizeEmail(model.Email);
		user.NormalizedUserName = _userManager.NormalizeName(model.UserName);

		var updateResult = await _userManager.UpdateAsync(user);
		if (!updateResult.Succeeded)
		{
			foreach (var error in updateResult.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return View(model);
		}

		var isCurrentlyAdmin = await _userManager.IsInRoleAsync(user, RoleNames.Administrator);
		if (model.IsAdmin && !isCurrentlyAdmin)
		{
			await _userManager.AddToRoleAsync(user, RoleNames.Administrator);
			await _userManager.RemoveFromRoleAsync(user, RoleNames.User);
		}
		else if (!model.IsAdmin && isCurrentlyAdmin)
		{
			await _userManager.RemoveFromRoleAsync(user, RoleNames.Administrator);
			await _userManager.AddToRoleAsync(user, RoleNames.User);
		}

		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(string id)
	{
		var currentUser = await _userManager.GetUserAsync(User);
		if (currentUser is not null && currentUser.Id == id)
		{
			TempData["ErrorMessage"] = "You cannot delete your own account.";
			return RedirectToAction(nameof(Index));
		}

		var user = await _userManager.FindByIdAsync(id);
		if (user is null)
		{
			return NotFound();
		}

		var result = await _userManager.DeleteAsync(user);
		if (!result.Succeeded)
		{
			TempData["ErrorMessage"] = string.Join("; ", result.Errors.Select(e => e.Description));
		}

		return RedirectToAction(nameof(Index));
	}
}
