namespace ArtemisBanking.Core.Application.Dtos.SavingAccount;

public class AccountTransferDto
{
    public string SourceAccountNumber { get; set; } = string.Empty;
    public string DestinationAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string UserId { get; set; } = string.Empty;
}