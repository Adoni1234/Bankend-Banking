namespace ArtemisBanking.Core.Application.Dtos.Login;

public class ResetPasswordRequestDto
{
    public required string UserId { get; set; }

    public required string Token { get; set; }

    public required string Password { get; set; }
}
