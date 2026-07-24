using ArtemisBanking.Core.Application.Dtos.Login;
using ArtemisBanking.Core.Application.Dtos.User;
using ArtemisBanking.Core.Domain.Common.Enums;

namespace ArtemisBanking.Core.Application.Interfaces;

public interface IBaseAccountService
{
    Task<Result<UserDto>> RegisterUser(UserSaveDto saveDto, string? origin, bool? isApi = false);
    Task<Result<UserDto>> EditUser(UserSaveDto saveDto, string? origin, bool? isApi = false);
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequestDto request, bool? isApi = false);
    Task<Result> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<Result> DeleteAsync(string id);
    Task<Result<UserDto>> GetUserByEmail(string email);
    Task<UserDto?> GetUserById(string id);
    Task<Result<List<UserDto>>> GetUsersByIds(List<string> ids);
    Task<Result<UserDto>> GetUserByUserName(string userName);
    Task<Result<UserDto>> GetByIdentityCardNumber(string identityCardNumber);
    Task<Result<List<UserDto>>> GetAllUser(bool? isActive = true);
    Task<Result<List<UserDto>>> GetAllUserOfRole(Roles role, bool isActive = true);
    Task<Result<List<string>>> GetAllUserIdsOfRole(Roles role, bool isActive = true);
    Task<int> CountUsers(Roles? role, bool? countOnlyWithStateOnly = null);
    Task<Result<List<string>>> GetAllUsersIds(bool isActive = true);
    Task<PaginatedData<UserDto>> GetAllTheUsersThatArentCommercesPaginated(string userId, int pageNumber = 1, int pageSize = 20, string? role = null);
    Task<Result> SetStateOnUser(string userId, bool state);
    Task<bool> ThisEmailExists(string email, string? id = null);
    Task<bool> ThisUsernameExists(string userName, string? id = null);
    Task<bool> ThisCommerceHaveAUserAssociated(int commerceId);
    Task<Result> ConfirmAccountAsync(string userId, string token);
    
}