using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Core.Application.ViewModels.CreditCard;

public class EditCreditCardLimitViewModel
{
    public required string CardNumber { get; set; } 
    [Required(ErrorMessage = "El limite de crédito es requerido")]
    public required decimal CreditLimit { get; set; }
}