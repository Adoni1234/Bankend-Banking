using ArtemisBanking.Core.Application.Dtos.LoanInstallment;
using ArtemisBanking.Core.Application.ViewModels.LoanInstallment;
using ArtemisBanking.Core.Application.ViewModels.User;

namespace ArtemisBanking.Core.Application.ViewModels.Loan;

// Modificado para que sea simetrico con la clase entidad
public class LoanViewModel
{
    public string Id { get; set; } // Recuerda, el ID de las cuentas y prestamo viene de la secuencia
    public required string ClientId { get; set; }
    public required string ApprovedByUserId { get; set; }
    public required decimal Amount { get; set; }
    public required int TermMonths { get; set; } // Plazos en meses
    public required decimal AnualRate { get; set; }
    public required bool Completed { get; set; }
    public required bool IsDue { get; set; } // si esta atrasada

    public required DateTime CreatedAt { get; set; } = DateTime.Now;
    public UserViewModel? Client { get; set; }
    public UserViewModel? ApprovedByUser { get; set; }
    public ICollection<LoanInstallmentViewModel> LoanInstallments { get; set; } = [];
}