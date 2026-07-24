namespace ArtemisBanking.Core.Application.Dtos.Email;

public class CashAdvanceEmailDto
{
    public string To { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal Amount { get; set; } 
    public string LastFourDigitsCard { get; set; } = string.Empty;
    public string LastFourDigitsAccount { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
}