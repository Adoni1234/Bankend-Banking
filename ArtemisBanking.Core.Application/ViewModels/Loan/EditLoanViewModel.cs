using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Core.Application.ViewModels.Loan;

public class EditLoanViewModel 
{
    public required string LoanId { get; set; }
    
    [Required(ErrorMessage = "El campo tasa anual es requerido")]
    public required decimal AnualRate { get; set; }
}