namespace AdoKt09AdminMvc.ViewModels.Admin.Users;

public class UserListItemViewModel
{
	public required string Id { get; init; }
	public required string UserName { get; init; }
	public required string Email { get; init; }
	public bool IsAdmin { get; init; }
}
