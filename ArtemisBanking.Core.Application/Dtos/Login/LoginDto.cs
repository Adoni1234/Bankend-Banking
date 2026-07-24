namespace ArtemisBanking.Core.Application.Dtos.Login;

public class LoginDto
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}