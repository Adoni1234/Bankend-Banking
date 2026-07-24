namespace ArtemisBanking.Core.Application.Dtos.Email;

public class ConfirmationEmailDto
{
    public string To { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty; 
    public string Token { get; set; } = string.Empty;
    public string ConfirmationUrl { get; set; } = string.Empty;
}