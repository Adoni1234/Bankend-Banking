using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Core.Application.ViewModels.CreditCard;

public class CreateCreditCardViewModel : IValidatableObject
{
    public required string ClientId { get; set; }

    [Required(ErrorMessage = "El campo limite de crédito es requerido")]
    public int CreditLimit { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CreditLimit <= 0)
        {
            yield return new ValidationResult(
                "El limite de crédito no puede ser cero",
                [nameof(CreditLimit)]
            );
        }
    }
}