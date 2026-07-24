using System.Linq.Expressions;
using ArtemisBanking.Core.Domain.Entities;

namespace ArtemisBanking.Core.Application.Interfaces;

// La interfaz realmente solamente usa 2 tipos, la implementacion de la intefaz si usa 3 tipos.
public interface IGenericService<in TKey, TDtoModel>
where TDtoModel: class
{
    Task<Result<TDtoModel>> GetByIdAsync(TKey id);
    Task<Result<List<TDtoModel>>> GetAllAsync();
    Task<Result<TDtoModel>> AddAsync(TDtoModel dto);
    Task<Result<List<TDtoModel>>> AddRangeAsync(List<TDtoModel> dto);
    Task<Result<TDtoModel>> UpdateAsync(TKey id, TDtoModel dto);
    Task<Result> DeleteAsync(TKey id);
    Task<Result<bool>> ExistsAsync(Func<IQueryable<TDtoModel>, bool> predicate);
}