using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using ArtemisBanking.Infrastructure.Persistence.Context;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories;

public class BeneficiaryRepository : GenericRepository<int, Beneficiary>, IBeneficiaryRepository
{
    public BeneficiaryRepository(ArtemisContext context) : base(context)
    {
    }
}