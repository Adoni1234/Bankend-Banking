using ArtemisBanking.Core.Application;
using ArtemisBanking.Core.Application.Dtos.SavingAccount;
using ArtemisBanking.Core.Application.Dtos.User;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingApi.Controllers;

[Authorize(Roles = $"{nameof(Roles.Admin)}")]
public class UsersController(
    IAccountServiceForWebApi accountServiceForWebApi,
    IMapper mapper,
    ISavingAccountService savingAccountService,
    ICommerceRepository commerceRepository)
    : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedData<UserDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsers([FromQuery] UsersApiFiltersDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst("uid")?.Value ?? "";
            var users =
                await accountServiceForWebApi.GetAllTheUsersThatArentCommercesPaginated(
                    userId,
                    dto.Page,
                    dto.PageSize,
                    dto.Role
                );
            return Ok(users);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("commerce")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedData<UserDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCommerces([FromQuery] CommerceFiltersDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst("uid")?.Value ?? "";

            var usersCommercesResult = await accountServiceForWebApi.GetAllUserOfRole(Roles.Commerce, false);
            if (usersCommercesResult.IsFailure)
            {
                return BadRequest(usersCommercesResult.GeneralError);
            }

            var paginatedCommerces = PaginatedData<UserDto>.Create(usersCommercesResult.Value!, dto.Page, dto.PageSize);
            return Ok(paginatedCommerces);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }


    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser(CreateApiUserDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                return BadRequest(ModelState);
            }

            if (dto.Role == Roles.Commerce)
            {
                return BadRequest("No se pueden crear comercios desde este endpoint");
            }
            
            var userNameExists = await accountServiceForWebApi.ThisUsernameExists(dto.UserName);
            var userEmailExists = await accountServiceForWebApi.ThisEmailExists(dto.Email);
            if (userEmailExists || userNameExists)
            {
                return Conflict("Este nombre de usuario o email ya existen");
            }

            var adminInSessionId = User.FindFirst("uid")?.Value ?? "";
            var userToCreate = new UserSaveDto
            {
                Id = "",
                IdentityCardNumber = dto.IdentityCardNumber,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.UserName,
                Password = dto.Password,
                Role = dto.Role.ToString()
            };
            
            var createUserResult = await accountServiceForWebApi.RegisterUser(userToCreate, "", true);

            if (createUserResult.IsFailure)
            {
                return BadRequest400WithErrorMessagesFromResult(createUserResult);
            }
            var createdUser = createUserResult.Value!;

            if (dto.Role == Roles.Client)
            {
                var accountResult =
                    await savingAccountService.CreateNewSavingAccountCard(
                        adminInSessionId,
                        createdUser.Id,
                        dto.InitialAmount ?? 0,
                        true);
                
                if (accountResult.IsFailure)
                {
                    return BadRequest400WithErrorMessagesFromResult(createUserResult);
                }
            }

            return Created();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("commerce/{commerceId:int}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCommerceUser([FromRoute]int commerceId ,[FromBody]CreateApiCommerceDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                return BadRequest(ModelState);
            }

            
            var userNameExists = await accountServiceForWebApi.ThisUsernameExists(dto.UserName);
            var userEmailExists = await accountServiceForWebApi.ThisUsernameExists(dto.Email);
            if (userEmailExists || userNameExists)
            {
                return Conflict("Este nombre de usuario o email ya existen");
            }
            
            var commerceExits = commerceRepository.GetAllQueryable().Any(c => c.Id == commerceId);
            if (!commerceExits)
            {
                return BadRequest("Este comercio no existe");
            }
            
            var commerceIdExists = await accountServiceForWebApi.ThisCommerceHaveAUserAssociated(commerceId);
            if (commerceIdExists)
            {
                return Conflict("Este comercio ya esta registrado a un usuario");
            }
            
            var adminInSessionId = User.FindFirst("uid")?.Value ?? "";
            var userToCreate = new UserSaveDto
            {
                Id = "",
                IdentityCardNumber = dto.IdentityCardNumber,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.UserName,
                Password = dto.Password,
                Role = nameof(Roles.Commerce),
                CommerceId = commerceId
            };
            
            var createUserResult = await accountServiceForWebApi.RegisterUser(userToCreate, "", true);
            if (createUserResult.IsFailure)
            {
                return BadRequest400WithErrorMessagesFromResult(createUserResult);
            }

            var createdUser = createUserResult.Value!;

            var accountResult = await savingAccountService.CreateNewSavingAccountCard(
                adminInSessionId,
                createdUser.Id,
                dto.InitialAmount,
                true);

            if (accountResult.IsFailure)
            {
                return BadRequest400WithErrorMessagesFromResult(createUserResult);
            }

            return Created();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUser([FromRoute] string id ,[FromBody] UpdateApiUserDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                return BadRequest(ModelState);
            }
            
            var userNameExists = await accountServiceForWebApi.ThisUsernameExists(dto.UserName, id);
            var userEmailExists = await accountServiceForWebApi.ThisUsernameExists(dto.Email, id);
            if (userEmailExists || userNameExists)
            {
                return Conflict("Este nombre de usuario o email ya existen");
            }

            var user = await accountServiceForWebApi.GetUserById(id);
            if (user is null)
            {
                return NotFound();
            }


            var update = mapper.Map<UserSaveDto>(dto);
            update.Id = user.Id;
            update.Role = user.Role;
            update.CommerceId = user.CommerceId;
            await accountServiceForWebApi.EditUser(update, "", true);

            if (update.Role == nameof(Roles.Client))
            {
                var mainAccountResult = await savingAccountService.GetMainAccountByUserIdAsync(id);
                if (mainAccountResult.IsFailure)
                {
                    return BadRequest400WithErrorMessagesFromResult(mainAccountResult);
                }
                await savingAccountService.DepositToAccountAsync(mainAccountResult.Value!.Id, dto.AdditionalAmount ?? 0);
            }
            
            return NoContent();

        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditUser([FromRoute] string id, [FromBody] ChangeApiUserStatusDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(dto);
            }
        
            var adminInSessionId = User.FindFirst("uid")?.Value ?? "";

            if (id == adminInSessionId)
            {
                return Forbid();
            }
            
            var user = await accountServiceForWebApi.GetUserById(id);
            if (user is null)
            {
                return NotFound();
            }

            await accountServiceForWebApi.SetStateOnUser(id, dto.Status);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserApiWithDetailsDto))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserWithDetails([FromRoute] string id)
    {
     
        var user =  await accountServiceForWebApi.GetUserById(id);
        if (user is null)
        {
            return NotFound();
        }
        var mainAccount = await savingAccountService.GetMainAccountByUserIdAsync(id);
        var userDetails = new UserApiWithDetailsDto
        {
            Id = user.Id,
            IdentityCardNumber = user.IdentityCardNumber,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            UserName = user.UserName,
            Role = user.Role,
            MainAccount = new SimpleSavingAccountApiDto
            {
                AccountNumber = mainAccount.Value!.Id, // Los Ids son los numeros de cuenta.
                Balance = mainAccount.Value!.Balance,
            }
        };
        
        return  Ok(userDetails);
    }
}