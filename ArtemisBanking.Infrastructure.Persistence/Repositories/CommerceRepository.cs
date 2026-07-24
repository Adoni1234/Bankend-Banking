using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using ArtemisBanking.Infrastructure.Persistence.Context;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories;

public class CommerceRepository : GenericRepository<int, Commerce>, ICommerceRepository
{
    public CommerceRepository(ArtemisContext context) : base(context)
    {
    }
}