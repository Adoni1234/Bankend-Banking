namespace ArtemisBanking.Core.Domain.Entities;

public class CreditCard
{
    public required string CardNumber { get; set; } // tambien es la PK
    public required string ClientId { get; set; } // UserID
    public required string ApprovedByUserId { get; set; }
    public required decimal CreditLimit { get; set; } // limite de la tarjetta
    public required decimal Balance { get; set; } 
    public required string CvcHashed  { get; set; }
    public required int ExpirationMonth { get; set; } // TinyInt
    public required int ExpirationYear { get; set; } // Smallint
    public required DateTime CreatedAt { get; set; } = DateTime.Now;
    public required bool IsActive { get; set; }

    public ICollection<CardTransaction> CardTransactions { get; set; } = [];
}