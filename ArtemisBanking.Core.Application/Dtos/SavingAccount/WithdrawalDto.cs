namespace ArtemisBanking.Core.Application.Dtos.SavingAccount;

public class WithdrawalDto
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TellerId { get; set; } = string.Empty; // ID del cajero que procesa el retiro
}