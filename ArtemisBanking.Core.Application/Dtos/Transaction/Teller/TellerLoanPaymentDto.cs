namespace ArtemisBanking.Core.Application.Dtos.Transaction.Teller;

public class TellerLoanPaymentDto
{
    public string SourceAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public string TellerId { get; set; } = string.Empty;
}