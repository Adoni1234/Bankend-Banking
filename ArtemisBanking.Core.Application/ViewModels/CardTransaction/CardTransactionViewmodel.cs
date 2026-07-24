using ArtemisBanking.Core.Application.Dtos.Commerce;
using ArtemisBanking.Core.Application.Dtos.CreditCard;
using ArtemisBanking.Core.Application.ViewModels.Commerce;
using ArtemisBanking.Core.Application.ViewModels.CreditCard;
using ArtemisBanking.Core.Domain.Common.Enums;

namespace ArtemisBanking.Core.Application.ViewModels.CardTransaction;

// Modificado para que sea simetrico con la clase entidad
public class CardTransactionViewmodel
{
    public required int Id { get; set; }
    public required string CreditCardNumber { get; set; }
    public required decimal Amount { get; set; }
    public int? CommerceId { get; set; } // nulable
    public required bool IsCashAdvance { get; set; }
    public required DateTime Date { get; set; } = DateTime.Now;
    public required CreditCardTransactionStatus Status { get; set; }
    public CommerceViewModel? Commerce { get; set; }
    public CreditCardViewModel? CreditCard { get; set; }
}