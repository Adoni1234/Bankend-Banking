namespace ArtemisBanking.Core.Application.Dtos.User;

public class UpdateApiUserDto
{
    public required string IdentityCardNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    public decimal? AdditionalAmount { get; set; }
}
