using ArtemisBanking.Core.Application.Dtos.User;

namespace ArtemisBanking.Core.Application.ViewModels.Loan;

public class LoanSummaryViewModel
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
    
    public required DateTime CreatedAt { get; set; } = DateTime.Now;
    public UserDto? Client { get; set; }
    public UserDto? ApprovedByUser { get; set; }
}