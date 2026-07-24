using ArtemisBanking.Core.Domain.Entities;

namespace ArtemisBanking.Core.Domain.Interfaces;

public interface ISavingAccountRepository : IGenericRepository<string, SavingAccount>
{
    Task<SavingAccount?> GetByAccountNumberAsync(string accountNumber);
    Task<bool> HasSufficientBalanceAsync(string accountNumber, decimal amount);
    Task<bool> AccountExistsAndIsActiveAsync(string accountNumber);

    Task SetStatus(string accountNumber, bool statusToSet);
}