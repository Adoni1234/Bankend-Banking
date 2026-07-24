namespace ArtemisBanking.Core.Application.Dtos.Loan;

public class SimpleLoanApiDto
{
    public required string Id { get; set; }
    public required string Client { get; set; }
    public required decimal Amount { get; set; }
    public required int TermMonths { get; set; } // Plazos en meses
    public required decimal AnualRate { get; set; }
    public required int InstallmentsCount { get; set; }
    public required int InstallmentsPaidCount { get; set; }
    public required decimal RemainingBalanceToPay { get; set; }
    public required bool Completed { get; set; }
    public required bool IsDue { get; set; } // si esta atrasada
}