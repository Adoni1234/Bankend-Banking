using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using ArtemisBanking.Infrastructure.Persistence.Context;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories;

public class CardTransactionRepository : GenericRepository<int, CardTransaction>,  ICardTransactionRepository
{
    public CardTransactionRepository(ArtemisContext context) : base(context)
    {
    }
}