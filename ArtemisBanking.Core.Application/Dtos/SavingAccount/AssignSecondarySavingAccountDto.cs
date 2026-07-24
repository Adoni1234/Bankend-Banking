using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Core.Application.Dtos.SavingAccount;

public class AssignSecondarySavingAccountDto
{
   
    [Required(ErrorMessage = "El ID del cliente es requerido")]
    public string ClientId { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "El monto inicial debe ser mayor o igual a 0")]
    public decimal InitialAmount { get; set; } = 0;
}