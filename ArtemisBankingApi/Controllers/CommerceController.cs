using ArtemisBanking.Core.Application;
using ArtemisBanking.Core.Application.Dtos.Commerce;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{nameof(Roles.Admin)}")]
public class CommerceController : BaseApiController
{
    private readonly ICommerceService _commerceService;

    public CommerceController(ICommerceService commerceService)
    {
        _commerceService = commerceService;
    }

    
    /// Obtiene un listado paginado de comercios ordenado del más reciente al más antiguo.
    /// Si no se pasan parámetros de paginación, devuelve el listado completo de comercios activos.
 
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedData<CommerceDto>))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CommerceDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCommerces([FromQuery] int? page = null, [FromQuery] int? pageSize = null)
    {
        try
        {
            // Si no se pasan parámetros, devolver todos los comercios activos
            if (!page.HasValue || !pageSize.HasValue)
            {
                var allResult = await _commerceService.GetAllActiveCommercesAsync();

                if (allResult.IsFailure)
                {
                    return BadRequest(new { error = allResult.GeneralError ?? "Error al obtener los comercios." });
                }

                return Ok(allResult.Value);
            }

            // Validar parámetros de paginación
            if (page.Value < 1)
            {
                return BadRequest(new { error = "El número de página debe ser mayor a 0." });
            }

            if (pageSize.Value < 1 || pageSize.Value > 100)
            {
                return BadRequest(new { error = "El tamaño de página debe estar entre 1 y 100." });
            }

            // Obtener comercios paginados
            var result = await _commerceService.GetCommercesPaginatedAsync(page.Value, pageSize.Value);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.GeneralError ?? "Error al obtener los comercios." });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error interno del servidor.", details = ex.Message });
        }
    }

  
    //Obtiene los detalles de un comercio según su identificador.
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CommerceDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommerceById(int id)
    {
        try
        {
            var result = await _commerceService.GetByIdAsync(id);

            if (result.IsFailure)
            {
                return NotFound(new { error = result.GeneralError ?? "Comercio no encontrado." });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error interno del servidor.", details = ex.Message });
        }
    }

    
    // Crea un nuevo comercio en el sistema.
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CommerceDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCommerce([FromBody] CreateCommerceDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var commerceDto = new CommerceDto
            {
                Id = 0,
                Name = dto.Name,
                Description = dto.Description,
                Logo = dto.Logo,
                IsActive = true,
                CardTransactions = []
            };

            var result = await _commerceService.AddAsync(commerceDto);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.GeneralError ?? "Error al crear el comercio." });
            }

            return CreatedAtAction(nameof(GetCommerceById), new { id = result.Value!.Id }, result.Value);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error interno del servidor.", details = ex.Message });
        }
    }

  
    // Actualiza los datos de un comercio existente.
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCommerce(int id, [FromBody] UpdateCommerceDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que el comercio existe
            var existingResult = await _commerceService.GetByIdAsync(id);
            if (existingResult.IsFailure)
            {
                return NotFound(new { error = "Comercio no encontrado." });
            }

            var existingCommerce = existingResult.Value!;
            var commerceDto = new CommerceDto
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                Logo = dto.Logo,
                IsActive = existingCommerce.IsActive,
                CardTransactions = existingCommerce.CardTransactions
            };

            var result = await _commerceService.UpdateAsync(id, commerceDto);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.GeneralError ?? "Error al actualizar el comercio." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error interno del servidor.", details = ex.Message });
        }
    }

    /* 
     * Desactiva o activa un comercio por su identificador.
     * Cuando se desactiva un comercio, todos los usuarios asociados se desactivan.
     * Alna
     */
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeCommerceStatus(int id, [FromBody] ChangeCommerceStatusDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que el comercio existe
            var existingResult = await _commerceService.GetByIdAsync(id);
            if (existingResult.IsFailure)
            {
                return NotFound(new { error = "Comercio no encontrado." });
            }

            var result = await _commerceService.ChangeStatusAsync(id, dto.Status);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.GeneralError ?? "Error al cambiar el estado del comercio." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error interno del servidor.", details = ex.Message });
        }
    }
}
