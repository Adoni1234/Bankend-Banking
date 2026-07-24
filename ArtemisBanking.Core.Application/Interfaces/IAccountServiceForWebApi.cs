using ArtemisBanking.Core.Application.Dtos.Login;

namespace ArtemisBanking.Core.Application.Interfaces
{
    public interface IAccountServiceForWebApi : IBaseAccountService
    {
        Task<Result<string>> AuthenticateAsync(LoginDto loginDto);
    }
}