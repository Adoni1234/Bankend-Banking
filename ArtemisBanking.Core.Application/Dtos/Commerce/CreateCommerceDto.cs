using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Core.Application.Dtos.Commerce;

public class CreateCommerceDto
{
    [Required(ErrorMessage = "El nombre del comercio es requerido")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 200 caracteres")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "La descripción del comercio es requerida")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "La descripción debe tener entre 1 y 1000 caracteres")]
    public required string Description { get; set; }

    [Required(ErrorMessage = "El logo del comercio es requerido")]
    [Url(ErrorMessage = "El logo debe ser una URL válida")]
    public required string Logo { get; set; }
}

