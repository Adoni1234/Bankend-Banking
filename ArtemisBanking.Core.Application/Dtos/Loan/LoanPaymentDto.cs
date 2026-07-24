namespace ArtemisBanking.Core.Application.Dtos.Loan;

public class LoanPaymentDto
{ 
    public int LoanId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string UserId { get; set; } = string.Empty;
}