using ArtemisBanking.Core.Application.Dtos.Loan;

namespace ArtemisBanking.Core.Application.Dtos.Transaction;

public class LoanDisbursementTransactionDto
{
    public required string SourceLoanNumber { get; set; }
    public required string DestinationAccountNumber { get; set; }
    public required string AprovedByAdminId { get; set; }
    public required decimal Amount { get; set; }
}