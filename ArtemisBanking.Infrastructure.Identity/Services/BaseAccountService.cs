using System.Text;
using ArtemisBanking.Core.Application;
using ArtemisBanking.Core.Application.Dtos.Email;
using ArtemisBanking.Core.Application.Dtos.Login;
using ArtemisBanking.Core.Application.Dtos.User;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Identity.Services
{
    public class BaseAccountService : IBaseAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        
        public BaseAccountService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }
        
        // En el ModelState, los Empty strings serán los errores generales
        public virtual async Task<Result<UserDto>> RegisterUser(UserSaveDto saveDto, string? origin, bool? isApi = false)
        {
            var userWithSameUserName = await _userManager.FindByNameAsync(saveDto.UserName);
            if (userWithSameUserName != null)
            {
                return Result<UserDto>.Fail($"this username: {saveDto.UserName} is already taken.");
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(saveDto.Email);
            if (userWithSameEmail != null)
            {
                return Result<UserDto>.Fail($"this email: {saveDto.Email} is already taken.");
            }
            
            var userWithSameIdentityCardNumber = await _userManager.Users
                .FirstOrDefaultAsync(u => u.IdentityCardNumber == saveDto.IdentityCardNumber);
            if (userWithSameIdentityCardNumber != null)
            {
                
                return Result<UserDto>.Fail($"This Identity Card Number: {saveDto.IdentityCardNumber} is already taken.");
            }

            userWithSameIdentityCardNumber = await _userManager.Users
                .FirstOrDefaultAsync(u => u.IdentityCardNumber == saveDto.IdentityCardNumber);
            if (userWithSameIdentityCardNumber != null)
            {
                
                return Result<UserDto>.Fail($"This Identity Card Number: {saveDto.IdentityCardNumber} is already taken.");
            }
            
            AppUser user = new AppUser
            {
                IdentityCardNumber = saveDto.IdentityCardNumber,
                FirstName = saveDto.FirstName,
                LastName = saveDto.LastName,
                Email = saveDto.Email,
                UserName = saveDto.UserName,
                EmailConfirmed = false,
                RegisteredAt = DateTime.Now,
            };

            var result = await _userManager.CreateAsync(user, saveDto.Password);
            if (!result.Succeeded)
            {
                return Result<UserDto>.Fail(result.Errors.Select(s => s.Description).ToList());
            }
            
            await _userManager.AddToRoleAsync(user, saveDto.Role);
            
            if (isApi != null && !isApi.Value)
            {
                string verificationUri = await GetVerificationEmailUri(user, origin ?? "");
                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = saveDto.Email,
                    HtmlBody = $"Please confirm your account visiting this URL {verificationUri}",
                    Subject = "Confirm registration"
                });
            }
            else
            {
                string? verificationToken = await GetVerificationEmailToken(user);
                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = saveDto.Email,
                    HtmlBody = $"Please confirm your account use this token {verificationToken}",
                    Subject = "Confirm registration"
                });
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
                RegisteredAt = user.RegisteredAt,
                Role = rolesList[0], // Asumimos que en este sistema solamente se tiene un rol
                IdentityCardNumber = user.IdentityCardNumber 
            };
            
            return Result<UserDto>.Ok(userDto);
        }
        
        
        public virtual async Task<Result<UserDto>> EditUser(UserSaveDto saveDto, string? origin, bool? isApi = false)
        {

            var userWithSameUserName = await _userManager.Users.FirstOrDefaultAsync(w => w.UserName == saveDto.UserName && w.Id != saveDto.Id);
            if (userWithSameUserName != null)
            {
                return Result<UserDto>.Fail($"this username: {saveDto.UserName} is already taken.");
                
            }

            var userWithSameEmail = await _userManager.Users.FirstOrDefaultAsync(w => w.Email == saveDto.Email && w.Id != saveDto.Id);
            if (userWithSameEmail != null)
            {
                return Result<UserDto>.Fail($"this email: {saveDto.Email} is already taken.");
            }
            
            var userWithSameIdentityCardNumber = await _userManager.Users
                .FirstOrDefaultAsync(u => u.IdentityCardNumber == saveDto.IdentityCardNumber && u.Id != saveDto.Id);
            if (userWithSameIdentityCardNumber != null)
            {
                
                return Result<UserDto>.Fail($"This Identity Card Number: {saveDto.IdentityCardNumber} is already taken.");
            }

            var user = await _userManager.FindByIdAsync(saveDto.Id);

            if (user == null)
            {
                return Result<UserDto>.Fail( $"There is no account registered with this user");
            }

            user.IdentityCardNumber = saveDto.IdentityCardNumber;
            user.FirstName = saveDto.FirstName;
            user.LastName = saveDto.LastName;
            user.UserName = saveDto.UserName;
            user.EmailConfirmed = user.EmailConfirmed && user.Email == saveDto.Email;
            user.Email = saveDto.Email;
            user.CommerceId = saveDto.CommerceId;

            if (!string.IsNullOrWhiteSpace(saveDto.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resultChange = await _userManager.ResetPasswordAsync(user, token, saveDto.Password);

                if (!resultChange.Succeeded)
                {
                    return Result<UserDto>.Fail(resultChange.Errors.Select(s => s.Description).ToList());
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Result<UserDto>.Fail(result.Errors.Select(s => s.Description).ToList());
            }
           
            var rolesList = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, rolesList.ToList());
            await _userManager.AddToRoleAsync(user, saveDto.Role);
            
            var updatedRolesList = await _userManager.GetRolesAsync(user);
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName,
                IsVerified = user.EmailConfirmed,
                RegisteredAt = user.RegisteredAt,
                CommerceId = user.CommerceId,
                Role = updatedRolesList[0],// un solo rol por usuario
                IdentityCardNumber = user.IdentityCardNumber
            };

            return Result<UserDto>.Ok(userDto);
        }

        public virtual async Task<Result> ForgotPasswordAsync(ForgotPasswordRequestDto request, bool? isApi = false)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
            {
                return Result.Fail($"There is no account registered with this username {request.UserName}");
            }

            user.EmailConfirmed = false;
            await _userManager.UpdateAsync(user);

            if (isApi != null && !isApi.Value)
            {
                var resetUri = await GetResetPasswordUri(user, request.Origin ?? "");
                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = user.Email,
                    HtmlBody = $"Please reset your password account visiting this URL {resetUri}",
                    Subject = "Reset password"
                });
            }
            else
            {
                string? resetToken = await GetResetPasswordToken(user);
                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = user.Email,
                    HtmlBody = $"Please reset your password account use this token {resetToken}",
                    Subject = "Reset password"
                });
            }

            return Result.Ok();
        }

        public virtual async Task<Result> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            
            if (user == null)
            {
                return Result.Fail($"There is no account registered with this user");
            }

            var token= Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded)
            {
                return Result<UserDto>.Fail(result.Errors.Select(s => s.Description).ToList());
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return Result.Ok();
        }
        public virtual async Task<Result> DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return Result.Fail($"There is no account registered with this user");
            }
            await _userManager.DeleteAsync(user);

            return Result.Ok();
        }

        public virtual async Task<Result<UserDto>> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result<UserDto>.Fail("There is no account registered with this user");
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
                CommerceId = user.CommerceId,
                RegisteredAt = user.RegisteredAt,
                Role = rolesList[0],
                IdentityCardNumber = user.IdentityCardNumber
            };
            return Result<UserDto>.Ok(userDto);
        }
        
        public virtual async Task<UserDto?> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            
            if (user == null)
            {
                return null;
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
                CommerceId = user.CommerceId,
                RegisteredAt = user.RegisteredAt,
                Role = rolesList[0],
                IdentityCardNumber = user.IdentityCardNumber
            };
            
            return userDto;
        }

        public virtual async Task<Result<List<UserDto>>> GetUsersByIds(List<string> ids)
        {
            var users = await _userManager.Users.Where(u => ids.Contains(u.Id)).ToListAsync();

            var usersDtos = users.Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName,
                IsVerified = user.EmailConfirmed,
                CommerceId = user.CommerceId,
                RegisteredAt = user.RegisteredAt,
                Role = _userManager.GetRolesAsync(user)
                           .Result.FirstOrDefault() ??
                       "",
                IdentityCardNumber = user.IdentityCardNumber
            }).ToList();
            return Result<List<UserDto>>.Ok(usersDtos);
        }

        public virtual async Task<Result<UserDto>> GetUserByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return Result<UserDto>.Fail("There is no account registered with this user");
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
                CommerceId = user.CommerceId,
                RegisteredAt = user.RegisteredAt,
                Role = rolesList[0],
                IdentityCardNumber = user.IdentityCardNumber
            };

            return Result<UserDto>.Ok(userDto);
        }

        public async Task<Result<UserDto>> GetByIdentityCardNumber(string identityCardNumber)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.IdentityCardNumber == identityCardNumber);

            if (user == null)
            {
                return Result<UserDto>.Fail("There is no account registered with that identity card number");
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
                RegisteredAt = user.RegisteredAt,
                CommerceId = user.CommerceId,
                Role = rolesList[0],
                IdentityCardNumber = user.IdentityCardNumber
            };

            return  Result<UserDto>.Ok(userDto);
        }

        public virtual async Task<Result<List<UserDto>>> GetAllUser(bool? isActive = true)
        {
            List<UserDto> listUsersDtos = [];

            var users = _userManager.Users;

            if (isActive != null && isActive == true)
            {
                users = users.Where(w => w.EmailConfirmed);
            }

            var listUser = await users.ToListAsync();

            foreach (var user in listUser)
            {
                var roleList = await _userManager.GetRolesAsync(user);
                listUsersDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName,
                    IsVerified = user.EmailConfirmed,
                    RegisteredAt = user.RegisteredAt,
                    CommerceId = user.CommerceId,
                    Role = roleList[0],
                    IdentityCardNumber = user.IdentityCardNumber
                });
            }
            return Result<List<UserDto>>.Ok(listUsersDtos);
        }

        public async Task<Result<List<UserDto>>> GetAllUserOfRole(Roles role, bool isActive = true)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.ToString());
            if (isActive)
            {
                usersInRole = usersInRole.Where(u => u.EmailConfirmed).ToList();
            }
            
            List<UserDto> listUsersDtos = [];
            listUsersDtos.AddRange(usersInRole.Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName,
                IsVerified = user.EmailConfirmed,
                RegisteredAt = user.RegisteredAt,
                CommerceId = user.CommerceId,
                Role = role.ToString(),
                IdentityCardNumber = user.IdentityCardNumber
            }));
            return Result<List<UserDto>>.Ok(listUsersDtos);
        }

        public async Task<Result<List<string>>> GetAllUserIdsOfRole(Roles role, bool isActive = true)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.ToString());
            if (isActive)
            {
                usersInRole = usersInRole.Where(u => u.EmailConfirmed).ToList();
            }
            
            var usersIds =  usersInRole.Select(u => u.Id).ToList();
            return Result<List<string>>.Ok(usersIds);
        }

        public async Task<int> CountUsers(Roles? role, bool? onlyActive = null)
        {
            List<AppUser> users;

            if (role != null)
                users = (await _userManager.GetUsersInRoleAsync(role.ToString())).ToList();
            else
                users = await _userManager.Users.ToListAsync();

            if (onlyActive == true)
                return users.Count(u => u.EmailConfirmed);

            if (onlyActive == false)
                return users.Count(u => !u.EmailConfirmed);

            return users.Count;
        }


        public async Task<Result<List<string>>> GetAllUsersIds(bool isActive = true)
        {
            var users = _userManager.Users;
            if (isActive)
            {
                users = users.Where(u => u.EmailConfirmed);
            }

            var usersIds = await users.Select(u => u.Id).ToListAsync();
            return Result<List<string>>.Ok(usersIds);
        }



        public virtual async Task<Result> ConfirmAccountAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                return Result<UserDto>.Fail("There is no account registered with this user");
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            
            if (!result.Succeeded)
            {

                return Result<UserDto>.Fail($"An error occurred while confirming this email {user.Email}");
            }
            
            return Result.Ok();
        }
        
        
    public async Task<PaginatedData<UserDto>> GetAllTheUsersThatArentCommercesPaginated(string userId,
        int pageNumber = 1, int pageSize = 20, string? role = null)
    {
        var commerces = await _userManager.GetUsersInRoleAsync(nameof(Roles.Commerce));

        var allUsers = new List<AppUser>();

        if (string.IsNullOrWhiteSpace(role))
        {
            allUsers = await _userManager.Users
                .Where(u => u.Id != userId)
                .ToListAsync();           
        }
        else
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            allUsers = usersInRole.ToList();
        }

        var usersToReturn = allUsers.Except(commerces).OrderByDescending(u => u.RegisteredAt);
        var userDtos = new List<UserDto>();
        foreach (var user in usersToReturn)
        {
            var roles =  await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName,
                IsVerified = user.EmailConfirmed,
                CommerceId = user.CommerceId,
                Role = roles.Single(),
                IdentityCardNumber = user.IdentityCardNumber,
                RegisteredAt = user.RegisteredAt 
            });
        }
        
        var data = PaginatedData<UserDto>.Create(userDtos, pageNumber, pageSize);
        return data;
    }

    public async Task<Result> SetStateOnUser(string userId, bool state)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null) 
            return Result.Fail($"There is no account registered with this username: {userId}");
        
        user.EmailConfirmed = state;
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return Result.Fail(updateResult.Errors.Select(s => s.Description).ToList());
        }
        
        return Result.Ok();
    }

    public async Task<bool> ThisEmailExists(string email, string? id = null)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            return await _userManager.Users.AnyAsync(u => u.Email == email && u.Id != id);
        }

        return await _userManager.Users.AnyAsync(u => u.Email == email);
    }
    
    public async Task<bool> ThisUsernameExists(string userName, string? id)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            return await _userManager.Users.AnyAsync(u => u.UserName == userName && u.Id != id);
        }

        return await _userManager.Users.AnyAsync(u => u.UserName == userName);
    }

    public async Task<bool> ThisCommerceHaveAUserAssociated(int commerceId)
    {
        return await _userManager.Users.AnyAsync(u => u.CommerceId == commerceId); ;
    }
    
    #region "Protected methods"

        protected async Task<string> GetVerificationEmailUri(AppUser user, string origin)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Login/ConfirmEmail";
            var completeUrl = new Uri(string.Concat(origin, "/", route));// origin = https://localhost:58296 route=Login/ConfirmEmail
            var verificationUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri.ToString(), "token", token);

            return verificationUri;
        }

        protected async Task<string?> GetVerificationEmailToken(AppUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }
        protected async Task<string> GetResetPasswordUri(AppUser user, string origin)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Login/ResetPassword";
            var completeUrl = new Uri(string.Concat(origin, "/", route));// origin = https://localhost:58296 route=Login/ConfirmEmail
            var resetUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            resetUri = QueryHelpers.AddQueryString(resetUri.ToString(), "token", token);

            return resetUri;
        }

        protected async Task<string?> GetResetPasswordToken(AppUser user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }
        
       #endregion
    }
}
