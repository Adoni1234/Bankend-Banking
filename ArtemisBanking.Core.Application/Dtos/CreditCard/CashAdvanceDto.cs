namespace ArtemisBanking.Core.Application.Dtos.CreditCard;

public class CashAdvanceDto
{
    
    public int CreditCardId { get; set; }
    public string DestinationAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string UserId { get; set; } = string.Empty;
}