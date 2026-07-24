using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Core.Application.Dtos.CardTransaction;

public class ProcessCardTransactionDto
{
    [Required(ErrorMessage = "El número de tarjeta es requerido")]
    public string CreditCardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El monto es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "El mes de expiración es requerido")]
    [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12")]
    public int ExpirationMonth { get; set; }

    [Required(ErrorMessage = "El año de expiración es requerido")]
    public int ExpirationYear { get; set; }

    [Required(ErrorMessage = "El CVC es requerido")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "El CVC debe tener 3 dígitos")]
    public string Cvc { get; set; } = string.Empty;

    public bool IsCashAdvance { get; set; } = false;
}

