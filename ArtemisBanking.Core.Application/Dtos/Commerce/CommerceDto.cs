using ArtemisBanking.Core.Application.Dtos.CardTransaction;

namespace ArtemisBanking.Core.Application.Dtos.Commerce;

// Modificado para que sea simetrico con la clase entidad
public class CommerceDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Logo { get; set; }
    public required bool IsActive { get; set; }

    public ICollection<CardTransactionDto> CardTransactions { get; set; } = [];
}