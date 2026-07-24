using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using ArtemisBanking.Infrastructure.Persistence.Context;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories;

public class SavingAccountRepository(
    ArtemisContext context,
    IIdentifierService identifierService)
    : GenericRepository<string, SavingAccount>(context), ISavingAccountRepository
{
    private readonly IIdentifierService _identifierService = identifierService;

    public override async Task<SavingAccount> AddAsync(SavingAccount entity)
    {
        entity.Id = await _identifierService.GenerateIdentifier(); // Genera del ID de 9 digitos 
        return await base.AddAsync(entity);
    }

    public async Task<SavingAccount?> GetByAccountNumberAsync(string accountNumber)
    {
        // El accountNumber es el Id (PK) de SavingAccount
        return await GetByIdAsync(accountNumber);
    }

    public async Task<bool> HasSufficientBalanceAsync(string accountNumber, decimal amount)
    {
        var account = await GetByIdAsync(accountNumber);
        return account != null && account.IsActive && account.Balance >= amount;
    }

    public async Task<bool> AccountExistsAndIsActiveAsync(string accountNumber)
    {
        var account = await GetByIdAsync(accountNumber);
        return account != null && account.IsActive;
    }

    public async Task SetStatus(string accountNumber, bool statusToSet)
    {
        var account = await GetByIdAsync(accountNumber);
        if (account != null)
        {
            account.IsActive = statusToSet;
            await Context.SaveChangesAsync();
        }
    }
}