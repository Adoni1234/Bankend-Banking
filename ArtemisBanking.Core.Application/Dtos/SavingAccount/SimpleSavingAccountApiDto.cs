namespace ArtemisBanking.Core.Application.Dtos.SavingAccount;

public class SimpleSavingAccountApiDto
{
    public required string AccountNumber { get; set; }
    public required decimal Balance { get; set; }
}