using ArtemisBanking.Core.Application.Dtos.LoanInstallment;
using ArtemisBanking.Core.Application.Dtos.User;

namespace ArtemisBanking.Core.Application.Dtos.Loan;

public class LoanSummaryDto
{
    public required string Id { get; set; } // Recuerda, el ID de las cuentas y prestamo viene de la secuencia
    public required string ClientId { get; set; }
    public required string ApprovedByUserId { get; set; }
    public required decimal Amount { get; set; }
    public required int TermMonths { get; set; } // Plazos en meses
    public required int InstallmentsCount { get; set; }
    public required int InstallmentsPaidCount { get; set; }
    public required decimal RemainingBalanceToPay { get; set; }
    public required decimal AnualRate { get; set; }
    public required bool Completed { get; set; }
    public required bool IsDue { get; set; } // si esta atrasada
    public DateTime CreatedAt { get; set; }
    
    public UserDto? Client { get; set; }
    public UserDto? ApprovedByUser { get; set; }
}