using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Core.Application.ViewModels.Teller;

public class DepositViewModel
{
    [Required(ErrorMessage = "El número de cuenta es requerido")]
    [Display(Name = "Número de Cuenta")]
    public string AccountNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El monto es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    [Display(Name = "Monto")]
    public decimal Amount { get; set; }

    [Display(Name = "Descripción")]
    public string? Description { get; set; }
}

