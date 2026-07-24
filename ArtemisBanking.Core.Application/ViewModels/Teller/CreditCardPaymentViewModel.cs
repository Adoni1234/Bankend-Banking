using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Core.Application.ViewModels.Teller;

public class CreditCardPaymentViewModel
{
    [Required(ErrorMessage = "El número de cuenta origen es requerido")]
    [Display(Name = "Número de Cuenta Origen")]
    public string SourceAccountNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de tarjeta de crédito es requerido")]
    [Display(Name = "Número de Tarjeta de Crédito")]
    public string CreditCardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El monto es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    [Display(Name = "Monto")]
    public decimal Amount { get; set; }

    [Display(Name = "Descripción")]
    public string? Description { get; set; }
}

