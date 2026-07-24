using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Core.Application.Dtos.CardTransaction;

public class ProcessPaymentDto
{
    [Required(ErrorMessage = "El número de tarjeta es requerido")]
    [StringLength(16, MinimumLength = 16, ErrorMessage = "El número de tarjeta debe tener 16 dígitos")]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "El número de tarjeta debe contener solo dígitos")]
    public required string CardNumber { get; set; }

    [Required(ErrorMessage = "El mes de expiración es requerido")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "El mes de expiración debe tener 2 dígitos")]
    [RegularExpression(@"^(0[1-9]|1[0-2])$", ErrorMessage = "El mes debe estar entre 01 y 12")]
    public required string MonthExpirationCard { get; set; }

    [Required(ErrorMessage = "El año de expiración es requerido")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "El año de expiración debe tener 4 dígitos")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "El año debe contener solo dígitos")]
    public required string YearExpirationCard { get; set; }

    [Required(ErrorMessage = "El CVC es requerido")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "El CVC debe tener 3 dígitos")]
    [RegularExpression(@"^\d{3}$", ErrorMessage = "El CVC debe contener solo dígitos")]
    public required string CVC { get; set; }

    [Required(ErrorMessage = "El monto de la transacción es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public required decimal TransactionAmount { get; set; }
}

