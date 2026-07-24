using ArtemisBanking.Core.Domain.Common.Enums;

namespace ArtemisBanking.Core.Domain.Entities;

public class CardTransaction
{
    public int Id { get; set; } // Se genera automáticamente por la base de datos
    
    public required string CreditCardNumber { get; set; }
    public required decimal Amount { get; set; }
    public int? CommerceId { get; set; } // nulable
    public required bool IsCashAdvance { get; set; }
    public required DateTime Date { get; set; } = DateTime.Now;
    public required CreditCardTransactionStatus Status { get; set; }
    
    public Commerce? Commerce { get; set; }
    public CreditCard? CreditCard { get; set; }
}