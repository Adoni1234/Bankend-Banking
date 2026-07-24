using System.Text;
using ArtemisBanking.Core.Application;
using ArtemisBanking.Core.Application.Dtos.Login;
using ArtemisBanking.Core.Application.Dtos.User;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Identity.Services;

public class AccountServiceForWebApp : BaseAccountService, IAccountServiceForWebApp
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailService _emailService;


    // En el ModelState, los Empty strings serán los errores generales
    public AccountServiceForWebApp(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
        IEmailService emailService) : base(userManager, signInManager, emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
    }
    public async Task<Result<UserDto>> AuthenticateAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByNameAsync(loginDto.UserName);

        if (user == null)
        {
            return Result<UserDto>.Fail( $"There is no account registered with this username: {loginDto.UserName}");
        }

        if (!user.EmailConfirmed)
        {
            return Result<UserDto>.Fail($"This account {loginDto.UserName} is not active, you should check your email");
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                string error =
                    $"Your account {loginDto.UserName} has been locked due to multiple failed attempts." +
                    $" Please try again in 10 minutes. If you don’t remember your password, " +
                    $"you can go through the password reset process.";
                
                return Result<UserDto>.Fail(error);
            }
                
            return Result<UserDto>.Fail($"these credentials are invalid for this user: {user.UserName}");
        }

        var rolesList = await _userManager.GetRolesAsync(user);
        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? "",
            UserName = user.UserName ?? "",
            FirstName = user.FirstName ?? "",
            LastName = user.LastName,
            IsVerified = user.EmailConfirmed,
            Role = rolesList[0],
            IdentityCardNumber = user.IdentityCardNumber, 
            RegisteredAt = user.RegisteredAt
        };

        return Result<UserDto>.Ok(userDto);
    }
   
    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }


}



