namespace ArtemisBanking.Core.Application.Dtos.Email;

public class CardTransactionEmailDto
{
    public string To { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal Amount { get; set; } 
    public string LastFourDigitsCard { get; set; } = string.Empty;
    public string CommerceName { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
}