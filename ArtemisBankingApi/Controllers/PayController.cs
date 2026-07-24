
using ArtemisBanking.Core.Application.Dtos.CardTransaction;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingApi.Controllers;

[ApiController]
[Route("pay")]
[Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.Commerce)}")]
public class PayController(
    ICardTransactionService cardTransactionService,
    ICommerceService commerceService,
    IAccountServiceForWebApi accountService)
    : BaseApiController
{
    /*
     * Obtiene un listado paginado de las transacciones registradas para un comercio.
     * Si el usuario es Commerce, el commerceId se obtiene del token JWT.
     * Si el usuario es Admin, el commerceId debe proporcionarse en la URL. - Alna
     */
   
    [HttpGet("get-transactions/{commerceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTransactions(
        int commerceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Obtener el ID del usuario autenticado desde el token JWT
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = "No se pudo identificar al usuario autenticado." });
            }

            // Obtener el rol del usuario
            var user = await accountService.GetUserById(userId);
            if (user == null)
            {
                return Unauthorized(new { error = "Usuario no encontrado." });
            }

            int finalCommerceId = commerceId;

            // Si el rol es Commerce, obtener el commerceId del token JWT
            if (user.Role == nameof(Roles.Commerce))
            {
                var commerceIdResult = await commerceService.GetCommerceIdByUserIdAsync(userId);
                if (commerceIdResult.IsFailure)
                {
                    return BadRequest(new { error = commerceIdResult.GeneralError ?? "No se pudo obtener el comercio del usuario." });
                }
                finalCommerceId = commerceIdResult.Value;
            }
            // Si el rol es Admin, usar el commerceId proporcionado en la URL

            // Validar parámetros de paginación
            if (page < 1)
            {
                return BadRequest(new { error = "El número de página debe ser mayor a 0." });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "El tamaño de página debe estar entre 1 y 100." });
            }

            // Obtener las transacciones
            var result = await cardTransactionService.GetByCommerceIdAsync(finalCommerceId, page, pageSize);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.GeneralError ?? "Error al obtener las transacciones." });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error interno del servidor.", details = ex.Message });
        }
    }

   
    /* Recibe los datos de un pago para ser procesado.
     * Si el usuario es Admin, el commerceId debe proporcionarse en la URL.
     * Si el usuario es Commerce, el commerceId se obtiene del token JWT. - Alna 
     */
    [HttpPost("process-payment/{commerceId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ProcessPayment(int commerceId, [FromBody] ProcessPaymentDto dto)
    {
        try
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

            // Obtener el rol del usuario
            var user = await accountService.GetUserById(userId);
            if (user == null)
            {
                return Unauthorized(new { error = "Usuario no encontrado." });
            }

            int finalCommerceId = commerceId;

            // Si el rol es Commerce, obtener el commerceId del token JWT
            if (user.Role == nameof(Roles.Commerce))
            {
                var commerceIdResult = await commerceService.GetCommerceIdByUserIdAsync(userId);
                if (commerceIdResult.IsFailure)
                {
                    return BadRequest(new { error = commerceIdResult.GeneralError ?? "No se pudo obtener el comercio del usuario." });
                }
                finalCommerceId = commerceIdResult.Value;
            }
            // Si el rol es Admin, usar el commerceId proporcionado en la URL

            // Convertir ProcessPaymentDto a ProcessCardTransactionDto
            var processDto = new ProcessCardTransactionDto
            {
                CreditCardNumber = dto.CardNumber,
                Amount = dto.TransactionAmount,
                ExpirationMonth = int.Parse(dto.MonthExpirationCard),
                ExpirationYear = int.Parse(dto.YearExpirationCard),
                Cvc = dto.CVC,
                IsCashAdvance = false
            };

            // Procesar el pago
            var result = await cardTransactionService.ProcessPaymentAsync(processDto, finalCommerceId);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.GeneralError ?? "Error al procesar el pago." });
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

