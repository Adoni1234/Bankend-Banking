using System.Security.Claims;
using ArtemisBanking.Core.Application.Dtos.CardTransaction;
using ArtemisBanking.Core.Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CommerceOnly")]

[ApiVersion(("1.0"))]
public class CardTransactionController(
    ICardTransactionService cardTransactionService,
    ICommerceService commerceService)
    : BaseApiController 
{
    /// <summary>
    /// Procesa una transacción de tarjeta de crédito
    /// </summary>
    /// <param name="dto">Datos de la transacción</param>
    /// <returns>Transacción procesada</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CardTransactionDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ProcessTransaction([FromBody] ProcessCardTransactionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Obtener el ID del usuario autenticado desde el token JWT
        var userId = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "No se pudo identificar al usuario autenticado." });
        }

        // Obtener el CommerceId del usuario
        var commerceIdResult = await commerceService.GetCommerceIdByUserIdAsync(userId);
        if (commerceIdResult.IsFailure)
        {
            return BadRequest(new { error = commerceIdResult.GeneralError ?? "No se pudo obtener el comercio del usuario." });
        }

        // Procesar la transacción
        var result = await cardTransactionService.ProcessCardTransactionAsync(dto, commerceIdResult.Value);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.GeneralError ?? "Error al procesar la transacción." });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene las transacciones del comercio autenticado
    /// </summary>
    /// <param name="page">Número de página (default: 1)</param>
    /// <param name="pageSize">Tamaño de página (default: 20)</param>
    /// <returns>Lista de transacciones</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<CardTransactionDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Obtener el ID del usuario autenticado desde el token JWT
        var userId = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "No se pudo identificar al usuario autenticado." });
        }

        // Obtener el CommerceId del usuario
        var commerceIdResult = await commerceService.GetCommerceIdByUserIdAsync(userId);
        if (commerceIdResult.IsFailure)
        {
            return BadRequest(new { error = commerceIdResult.GeneralError ?? "No se pudo obtener el comercio del usuario." });
        }

        // Obtener las transacciones
        var result = await cardTransactionService.GetByCommerceIdAsync(commerceIdResult.Value, page, pageSize);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.GeneralError ?? "Error al obtener las transacciones." });
        }

        return Ok(result.Value);
    }
}

