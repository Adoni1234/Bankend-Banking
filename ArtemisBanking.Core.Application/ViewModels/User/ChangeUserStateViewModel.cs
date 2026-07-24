namespace ArtemisBanking.Core.Application.ViewModels.User;

public class ChangeUserStateViewModel
{
    public required string UserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required bool State { get; set; }
}