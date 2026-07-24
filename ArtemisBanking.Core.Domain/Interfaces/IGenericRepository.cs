namespace ArtemisBanking.Core.Domain.Interfaces;

public interface IGenericRepository<TKey, TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TKey id);
    Task<List<TEntity>> GetAllAsync();
    Task<TEntity> AddAsync(TEntity entity);
    Task<List<TEntity>> AddRangeAsync(List<TEntity> entities);
    Task<TEntity?> UpdateAsync(TKey id,TEntity entity);
    Task DeleteAsync(TKey id);
    IQueryable<TEntity> GetAllQueryable();
    Task<int> SaveChangesAsync(); // lo necesito para hacer update de los installments
}