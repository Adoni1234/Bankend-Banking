using ArtemisBanking.Core.Application.Dtos.LoanInstallment;

namespace ArtemisBanking.Core.Application.Dtos.Loan;

public class LoanApiWithDetails
{
    public required string Id { get; set; }
    public required string ClientId { get; set; }
    public required decimal Amount { get; set; }
    public required int TermMonths { get; set; } // Plazos en meses
    public required decimal AnualRate { get; set; }
    public required bool Completed { get; set; }
    public required bool IsDue { get; set; } // si esta atrasada
    public List<SimpleLoanInstallmentApiDto> LoanInstallments { get; set; } = [];
}