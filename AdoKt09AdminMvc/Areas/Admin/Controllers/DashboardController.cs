using AdoKt09AdminMvc.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdoKt09AdminMvc.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class DashboardController : Controller
{
	public IActionResult Index()
	{
		return View();
	}
}
