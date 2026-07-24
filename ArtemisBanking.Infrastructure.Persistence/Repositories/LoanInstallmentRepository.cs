using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using ArtemisBanking.Infrastructure.Persistence.Context;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories;

public class LoanInstallmentRepository : GenericRepository<int, LoanInstallment>,  ILoanInstallmentRepository
{
    public LoanInstallmentRepository(ArtemisContext context) : base(context)
    {
    }
}