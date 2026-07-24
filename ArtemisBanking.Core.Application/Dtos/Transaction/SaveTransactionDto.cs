namespace ArtemisBanking.Core.Application.Dtos.Transaction;

public class SaveTransactionDto
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string Beneficiary { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; }  = string.Empty;
}