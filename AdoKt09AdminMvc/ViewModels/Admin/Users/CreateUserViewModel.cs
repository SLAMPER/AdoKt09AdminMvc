using System.ComponentModel.DataAnnotations;

namespace AdoKt09AdminMvc.ViewModels.Admin.Users;

public class CreateUserViewModel
{
	[Required]
	[StringLength(64)]
	public string UserName { get; set; } = string.Empty;

	[Required]
	[EmailAddress]
	public string Email { get; set; } = string.Empty;

	[Required]
	[DataType(DataType.Password)]
	public string Password { get; set; } = string.Empty;

	[Required]
	[DataType(DataType.Password)]
	[Compare(nameof(Password))]
	public string ConfirmPassword { get; set; } = string.Empty;

	public bool IsAdmin { get; set; }
}
