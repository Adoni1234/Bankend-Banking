namespace ArtemisBanking.Core.Domain.Entities;

public class Beneficiary
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string SavingAccountId { get; set; }
    
    public SavingAccount? SavingAccount { get; set; }
}