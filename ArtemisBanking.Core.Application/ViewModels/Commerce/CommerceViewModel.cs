using ArtemisBanking.Core.Application.Dtos.CardTransaction;
using ArtemisBanking.Core.Application.ViewModels.CardTransaction;

namespace ArtemisBanking.Core.Application.ViewModels.Commerce;

// Modificado para que sea simetrico con la clase entidad
public class CommerceViewModel
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Logo { get; set; }
    public required bool IsActive { get; set; }

    public ICollection<CardTransactionViewmodel> CardTransactions { get; set; } = [];
}