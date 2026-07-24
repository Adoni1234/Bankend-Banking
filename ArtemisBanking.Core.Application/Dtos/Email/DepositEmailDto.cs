namespace ArtemisBanking.Core.Application.Dtos.Email;

public class DepositEmailDto
{
    public string To { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string LastFourDigits { get; set; } = string.Empty;
    public DateTime DepositDate { get; set; }
}