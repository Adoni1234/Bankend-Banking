namespace ArtemisBanking.Core.Domain.Entities;

public class Commerce
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Logo { get; set; }
    public required bool IsActive { get; set; }

    public ICollection<CardTransaction> CardTransactions { get; set; } = [];
}