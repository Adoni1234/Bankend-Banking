using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Core.Application.Dtos.Commerce;

public class ChangeCommerceStatusDto
{
    [Required(ErrorMessage = "El estado es requerido")]
    public required bool Status { get; set; }
}

