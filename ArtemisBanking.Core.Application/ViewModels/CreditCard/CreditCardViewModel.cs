using ArtemisBanking.Core.Application.ViewModels.CardTransaction;
using ArtemisBanking.Core.Application.ViewModels.User;

namespace ArtemisBanking.Core.Application.ViewModels.CreditCard;

// Modificado para que sea simetrico con la clase entidad
public class CreditCardViewModel
{
    public string CardNumber { get; set; } // tambien es la PK
    public required string ClientId { get; set; } // UserID
    public required string ApprovedByUserId { get; set; }
    public required decimal CreditLimit { get; set; } // limite de la tarjetta
    public required decimal Balance { get; set; } // el mounto adeudado
    public required string CvcHashed  { get; set; }
    public required int ExpirationMonth { get; set; } // TinyInt
    public required int ExpirationYear { get; set; } // Smallint
    public required DateTime CreatedAt { get; set; } = DateTime.Now;
    public required bool IsActive { get; set; }
    
    public UserViewModel? CreatedByUser { get; set; }
    public UserViewModel? Client { get; set; }
    public ICollection<CardTransactionViewmodel> CardTransactions { get; set; } = [];
}