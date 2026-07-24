namespace ArtemisBanking.Core.Application.Dtos.CreditCard;

public class CreditCardPaymentDto
{
    public int CreditCardId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string UserId { get; set; } = string.Empty;
}