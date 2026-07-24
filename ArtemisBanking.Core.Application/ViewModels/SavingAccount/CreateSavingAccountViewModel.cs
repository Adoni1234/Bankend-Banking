using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Core.Application.ViewModels.SavingAccount;

public class CreateSavingAccountViewModel : IValidatableObject
{
    public required string ClientId { get; set; }

    [Required(ErrorMessage = "El campo limite de crédito es requerido")]
    public int InitialAmount { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (InitialAmount < 0)
        {
            yield return new ValidationResult(
                "El balance de la cuenta no puede ser cero",
                [nameof(InitialAmount)]
            );
        }
    }
}