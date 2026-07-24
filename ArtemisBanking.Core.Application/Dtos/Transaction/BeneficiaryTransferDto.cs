namespace ArtemisBanking.Core.Application.Dtos.Transaction;

public class BeneficiaryTransferDto
{
    public int BeneficiaryId { get; set; }
    public decimal Amount { get; set; }
    public string SourceAccountNumber { get; set; }  = string.Empty;
    public string UserId { get; set; } = string.Empty;
}