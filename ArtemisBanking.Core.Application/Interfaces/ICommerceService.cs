
using ArtemisBanking.Core.Application.Dtos.Commerce;

namespace ArtemisBanking.Core.Application.Interfaces;

public interface ICommerceService : IGenericService<int , CommerceDto> 
{
    Task<Result<int>> GetCommerceIdByUserIdAsync(string userId);
    Task<Result<PaginatedData<CommerceDto>>> GetCommercesPaginatedAsync(int page, int pageSize);
    Task<Result<List<CommerceDto>>> GetAllActiveCommercesAsync();
    Task<Result> ChangeStatusAsync(int id, bool status);
}