using AdoKt09AdminMvc.Data;
using AdoKt09AdminMvc.Infrastructure.Auth;
using AdoKt09AdminMvc.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AdoKt09AdminMvc.Infrastructure.Seed;

public static class IdentitySeed
{
	public static async Task SeedAsync(IServiceProvider serviceProvider)
	{
		var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
		await dbContext.Database.MigrateAsync();

		var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
		var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
		var adminOptions = serviceProvider.GetRequiredService<IOptions<SeedAdminOptions>>().Value;
		var adminUserName = adminOptions.Email;

		await EnsureRoleAsync(roleManager, RoleNames.Administrator);
		await EnsureRoleAsync(roleManager, RoleNames.User);

		var adminUser = await userManager.FindByEmailAsync(adminOptions.Email);
		if (adminUser is null)
		{
			adminUser = new AppUser
			{
				UserName = adminUserName,
				Email = adminOptions.Email,
				EmailConfirmed = true
			};

			var createResult = await userManager.CreateAsync(adminUser, adminOptions.Password);
			if (!createResult.Succeeded)
			{
				var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
				throw new InvalidOperationException($"Failed to create seed admin user: {errors}");
			}
		}
		else if (!string.Equals(adminUser.UserName, adminUserName, StringComparison.Ordinal))
		{
			adminUser.UserName = adminUserName;
			adminUser.NormalizedUserName = userManager.NormalizeName(adminUserName);
			adminUser.NormalizedEmail = userManager.NormalizeEmail(adminOptions.Email);

			var updateResult = await userManager.UpdateAsync(adminUser);
			if (!updateResult.Succeeded)
			{
				var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
				throw new InvalidOperationException($"Failed to update seed admin user: {errors}");
			}
		}

		if (!await userManager.IsInRoleAsync(adminUser, RoleNames.Administrator))
		{
			var addToRoleResult = await userManager.AddToRoleAsync(adminUser, RoleNames.Administrator);
			if (!addToRoleResult.Succeeded)
			{
				var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
				throw new InvalidOperationException($"Failed to assign admin role to seed user: {errors}");
			}
		}
	}

	private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
	{
		if (await roleManager.RoleExistsAsync(roleName))
		{
			return;
		}

		var result = await roleManager.CreateAsync(new IdentityRole(roleName));
		if (!result.Succeeded)
		{
			var errors = string.Join(", ", result.Errors.Select(e => e.Description));
			throw new InvalidOperationException($"Failed to create role '{roleName}': {errors}");
		}
	}
}
