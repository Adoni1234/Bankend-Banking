namespace ArtemisBanking.Core.Application.Dtos.Email;

public class TransactionEmailDto
{
    public string To { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public decimal Amount { get; set; } 
    public string AccountNumber { get; set; }  = string.Empty;
    public string LastFourDigits { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } 
    public string TransactionType { get; set; } = string.Empty;
}