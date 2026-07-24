using System.ComponentModel.DataAnnotations;
using ArtemisBanking.Core.Application.Enums;

namespace ArtemisBanking.Core.Application.Dtos.SavingAccount;

public class SavingApiAccountDto
{
   
    /// Cédula del cliente para filtrar cuentas
    public string? Cedula { get; set; }
    
    
    /// Estado de la cuenta: activo | cancelado
    public StateAccountApi? State { get; set; }
    
    /// Tipo de cuenta: principal | secundaria
    public TypeAccountApi? Type { get; set; }
    
    /// Número de página (default: 1)
    [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor a 0")]
    public int Page { get; set; } = 1;
}