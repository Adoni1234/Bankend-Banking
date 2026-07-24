namespace ArtemisBanking.Core.Application.Dtos.Transaction;

public class ExpressTransferDto
{
    public string DestinationAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string SourceAccountNumber { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}