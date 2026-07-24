namespace ArtemisBanking.Core.Application.Dtos.Transaction;

public class TransferConfirmationDto
{
    public string DestinationAccountNumber { get; set; } = string.Empty;
    public string BeneficiaryFullName { get; set; }  = string.Empty;
    public decimal Amount { get; set; }
    public string SourceAccountNumber { get; set; } = string.Empty;
}