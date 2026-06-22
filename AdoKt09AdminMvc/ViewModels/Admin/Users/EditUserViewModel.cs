using System.ComponentModel.DataAnnotations;

namespace AdoKt09AdminMvc.ViewModels.Admin.Users;

public class EditUserViewModel
{
	[Required]
	public string Id { get; set; } = string.Empty;

	[Required]
	[StringLength(64)]
	public string UserName { get; set; } = string.Empty;

	[Required]
	[EmailAddress]
	public string Email { get; set; } = string.Empty;

	public bool IsAdmin { get; set; }
}
