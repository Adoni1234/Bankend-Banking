using ArtemisBanking.Core.Domain.Entities;

namespace ArtemisBanking.Core.Domain.Interfaces;

public interface ICardTransactionRepository : IGenericRepository<int, CardTransaction>
{
    
}