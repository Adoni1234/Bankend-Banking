using ArtemisBanking.Core.Application.Dtos.CardTransaction;
using ArtemisBanking.Core.Application.Dtos.User;

namespace ArtemisBanking.Core.Application.Dtos.CreditCard;

// Modificado para que sea simetrico con la clase entidad
public class CreditCardDto
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
    
    public UserDto? CreatedByUser { get; set; }
    public UserDto? Client { get; set; }
    public ICollection<CardTransactionDto> CardTransactions { get; set; } = [];
}