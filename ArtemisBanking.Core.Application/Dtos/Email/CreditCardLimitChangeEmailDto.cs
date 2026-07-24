namespace ArtemisBanking.Core.Application.Dtos.Email;

public class CreditCardLimitChangeEmailDto
{
    public string To { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string LastFourDigits { get; set; } = string.Empty;
    public decimal NewLimit { get; set; }
}