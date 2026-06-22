using AdoKt09AdminMvc.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdoKt09AdminMvc.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
	: IdentityDbContext<AppUser, IdentityRole, string>(options)
{
}
