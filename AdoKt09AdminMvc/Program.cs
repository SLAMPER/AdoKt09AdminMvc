using AdoKt09AdminMvc.Data;
using AdoKt09AdminMvc.Infrastructure.Auth;
using AdoKt09AdminMvc.Infrastructure.Seed;
using AdoKt09AdminMvc.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AdoKt09AdminMvc;

public abstract class Program
{
	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
		                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

		builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseSqlite(connectionString));
		builder.Services.Configure<SeedAdminOptions>(builder.Configuration.GetSection("SeedAdmin"));

		builder.Services
			.AddIdentity<AppUser, IdentityRole>(options => { options.SignIn.RequireConfirmedAccount = false; })
			.AddEntityFrameworkStores<AppDbContext>()
			.AddDefaultTokenProviders()
			.AddDefaultUI();

		builder.Services.AddAuthorizationBuilder()
			.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
				policy.RequireRole(RoleNames.Administrator));

		builder.Services.AddControllersWithViews();
		builder.Services.AddRazorPages();

		var app = builder.Build();

		using (var scope = app.Services.CreateScope())
		{
			var services = scope.ServiceProvider;
			await IdentitySeed.SeedAsync(services);
		}

		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Home/Error");
			app.UseHsts();
		}

		app.UseHttpsRedirection();
		app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();

		app.MapStaticAssets();
		app.MapControllerRoute(
				"areas",
				"{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
			.WithStaticAssets();
		app.MapControllerRoute(
				"default",
				"{controller=Home}/{action=Index}/{id?}")
			.WithStaticAssets();
		app.MapRazorPages();

		await app.RunAsync();
	}
}
