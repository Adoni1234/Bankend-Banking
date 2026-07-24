using ArtemisBanking.Core.Application.Dtos.Login;
using ArtemisBanking.Core.Application.Dtos.User;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingApi.Controllers;

[Authorize (Roles = $"{nameof(Roles.Admin)},{nameof(Roles.Commerce)}")]
public class AccountController: BaseApiController
{
    private readonly IAccountServiceForWebApi _accountService;

    public AccountController(IAccountServiceForWebApi accountService)
    {
        _accountService = accountService;
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK,  Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _accountService.AuthenticateAsync(loginDto);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.GeneralError ?? "Error de autenticación" });
            }

            return Ok(new { token = result.Value });           
        }
        catch (Exception ex)
        {
            
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }

    }
    
    [HttpPost("confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Confirm([FromBody] ConfirmDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
        
            var result = await _accountService.ConfirmAccountAsync(dto.UserId, dto.Token);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.GeneralError ?? "Error de autenticación" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }

    }
    
    [HttpPost("get-reset-token")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetResetToken([FromBody] ResetTokenDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _accountService.ForgotPasswordAsync(new ForgotPasswordRequestDto
            {
                UserName = dto.Username,
                Origin = "",
            }, true);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.GeneralError ?? "Error de autenticación" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }

    }
    
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetToken([FromBody] ResetPasswordApiRequestDto dto)
    {
        try
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            
            if (dto.Password != dto.ConfirmPassword)
            {
                return BadRequest(new { message = "las contraseñas no coinciden" });
            }
            
            var result = await _accountService.ResetPasswordAsync(new ResetPasswordRequestDto
            {
                UserId = dto.UserId,
                Token = dto.Token,
                Password = dto.Password,
            });

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.GeneralError ?? "Error de autenticación" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }

    }
}

