using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Core.Application.ViewModels.Loan;

public class CreateLoanViewModel : IValidatableObject
{
    public required string ClientId { get; set; }
    [Required(ErrorMessage = "El campo plazo es requerido")]
    public required int TermMonth { get; set; }
    [Required(ErrorMessage = "El campo tasa anual es requerido")]
    public required decimal AnualRate { get; set; }
    
    [Required(ErrorMessage = "El campo capital es requerido")]
    public required decimal Amount { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validTermMonths = new int[] {6, 12, 18, 24, 30, 36, 42, 48, 54, 60};
        if (!validTermMonths.Contains(TermMonth))
        {
            yield return new ValidationResult(
                "Los plazos solamente pueden ser de: 6, 12, 18, 24, 30, 36, 42, 48, 54, 60",
                [nameof(TermMonth)]);
        }

        if (Amount <= 0)
        {
            yield return new ValidationResult(
                "El monto no puede ser un numero menor o igual a cero",
                [nameof(Amount)]
                );
        }
    }
}