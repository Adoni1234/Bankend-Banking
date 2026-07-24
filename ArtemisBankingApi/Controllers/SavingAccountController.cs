using ArtemisBanking.Core.Application;
using ArtemisBanking.Core.Application.Dtos.SavingAccount;
using ArtemisBanking.Core.Application.Dtos.Transaction;
using ArtemisBanking.Core.Application.Dtos.User;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingApi.Controllers;

[Authorize(Roles = $"{nameof(Roles.Admin)}")]
public class SavingAccountController (
    IAccountServiceForWebApi accountServiceForWebApi,
    IMapper mapper,ITransactionService transactionService,
    ISavingAccountService savingAccountService,
    ICommerceRepository commerceRepository)
    : BaseApiController
{

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedData<SavingAccountDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]

    public async Task<IActionResult> GetSavingAccounts([FromQuery] SavingApiAccountDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Unauthorized(ModelState);
            }

            if (dto.Page < 1)
            {
                return BadRequest(new { error = "El número de página debe ser mayor a 0." });
            }

            var result = await savingAccountService.GetSavingAccountsForApiAsync(dto);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.GeneralError ?? "Error al obtener las cuentas de ahorro." });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Error interno del servidor.", detail = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SavingAccountDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignSecondarySavingAccount([FromBody] AssignSecondarySavingAccountDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Obtener el ID del usuario autenticado desde el token JWT
            var adminUserId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(adminUserId))
            {
                return Unauthorized(new { error = "No se pudo identificar al usuario autenticado." });
            }

            var result = await savingAccountService.CreateNewSavingAccountCard(
                adminWhoApproved: adminUserId,
                clientId: dto.ClientId,
                initialAmount: dto.InitialAmount,
                isPrincipal: false);

            if (result.IsFailure)
            {
                // Verificar si es un conflicto (número de cuenta ya existe)
                if (result.GeneralError?.Contains("ya existe") == true ||
                    result.GeneralError?.Contains("duplicado") == true)
                {
                    return Conflict(new { error = result.GeneralError });
                }

                return BadRequest(new { error = result.GeneralError ?? "Error al asignar la cuenta de ahorro." });
            }
            return CreatedAtAction(
                nameof(GetSavingAccountByNumber), 
                new { accountNumber = result.Value!.Id }, 
                result.Value);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Error interno del servidor.", details = ex.Message });
        }
    }
    [HttpGet("{accountNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SavingAccountDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSavingAccountByNumber(string accountNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return BadRequest(new { error = "El número de cuenta es requerido." });
            }

            var result = await savingAccountService.GetByIdAsync(accountNumber);

            if (result.IsFailure)
            {
                return NotFound(new { error = result.GeneralError ?? "Cuenta de ahorro no encontrada." });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Error interno del servidor.", details = ex.Message });
        }
    }
    [HttpGet("{accountNumber}/transactions")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedData<TransactionDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTransactionsByAccount(
        string accountNumber,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validar parámetros
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return BadRequest(new { error = "El número de cuenta es requerido." });
            }

            if (page < 1)
            {
                return BadRequest(new { error = "El número de página debe ser mayor a 0." });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "El tamaño de página debe estar entre 1 y 100." });
            }

            // Verificar que la cuenta existe
            var accountResult = await savingAccountService.GetByIdAsync(accountNumber);
            if (accountResult.IsFailure)
            {
                return NotFound(new { error = "Cuenta no encontrada." });
            }

            // Obtener las transacciones de la cuenta
            var result = await transactionService.GetTransactionsByAccountAsync(
                accountNumber, 
                page, 
                pageSize);

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
}