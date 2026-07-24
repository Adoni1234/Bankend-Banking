using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArtemisBanking.Core.Application;
using ArtemisBanking.Core.Application.Dtos.Login;
using ArtemisBanking.Core.Application.Dtos.User;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Settings;
using ArtemisBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ArtemisBanking.Infrastructure.Identity.Services
{
    public class AccountServiceForWebApi : BaseAccountService, IAccountServiceForWebApi
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly JwtSettings _jwtSettings;

        public AccountServiceForWebApi(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            IEmailService emailService, IOptions<JwtSettings> jwtSettings) : base(userManager, signInManager,
            emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
        }
        
        public async Task<Result<string>> AuthenticateAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);

            if (user == null)
            {
                return Result<string>.Fail( $"There is no account registered with this username: {loginDto.UserName}");
            }

            if (!user.EmailConfirmed)
            {
                return Result<string>.Fail($"This account {loginDto.UserName} is not active, you should check your email");
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
                
                    return Result<string>.Fail(error);
                }
                
                return Result<string>.Fail($"these credentials are invalid for this user: {user.UserName}");
            }
        
            string token = new JwtSecurityTokenHandler().WriteToken(await GenerateJwtToken(user));
            return Result<string>.Ok(token);
        }


        public override async Task<Result<UserDto>> RegisterUser(UserSaveDto saveDto, string? origin, bool? isApi = false)
        {
            return await base.RegisterUser(saveDto, null, isApi);
        }

        public override async Task<Result<UserDto>> EditUser(UserSaveDto saveDto, string? origin, bool? isApi = false)
        {
            return await base.EditUser(saveDto, null, isApi);
        }


        private async Task<JwtSecurityToken> GenerateJwtToken(AppUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var rolesClaims = new List<Claim>();
            foreach (var role in roles)
            {
                rolesClaims.Add(new Claim("roles", role));
            }
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim("uid",user.Id ?? "")
            }.Union(userClaims).Union(rolesClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials
            );
            
            return jwtSecurityToken;
        }
   }
}
