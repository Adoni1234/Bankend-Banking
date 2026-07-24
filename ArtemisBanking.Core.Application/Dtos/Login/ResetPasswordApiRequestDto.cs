namespace ArtemisBanking.Core.Application.Dtos.Login;

public class ResetPasswordApiRequestDto
{
    public required string UserId { get; set; }
    public required string Token { get; set; }
    public required string Password { get; set; }    
    public required string ConfirmPassword { get; set; }    
}
