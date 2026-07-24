using ArtemisBanking.Core.Application.Dtos.Login;
using ArtemisBanking.Core.Application.Dtos.User;

namespace ArtemisBanking.Core.Application.Interfaces;

public interface IAccountServiceForWebApp : IBaseAccountService
{
    Task<Result<UserDto>> AuthenticateAsync(LoginDto loginDto);
    Task SignOutAsync();
}