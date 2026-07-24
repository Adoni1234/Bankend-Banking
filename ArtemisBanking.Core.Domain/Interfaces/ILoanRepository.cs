using ArtemisBanking.Core.Domain.Entities;

namespace ArtemisBanking.Core.Domain.Interfaces;

public interface ILoanRepository : IGenericRepository<string, Loan>
{
    Task<Loan?> GetByLoanNumberAsync(string loanNumber);
    Task<bool> LoanExistsAndIsActiveAsync(string loanNumber);
}