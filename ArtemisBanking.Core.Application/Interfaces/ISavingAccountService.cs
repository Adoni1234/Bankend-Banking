using ArtemisBanking.Core.Application.Dtos.SavingAccount;

namespace ArtemisBanking.Core.Application.Interfaces;

public interface ISavingAccountService : IGenericService<string, SavingAccountDto>
{ 
    //
    // Task<Result<List<SavingAccountDto>>> GetByUserIdAsync(string userId, bool? isActive = null, bool? isMainAccount = null);
    // Task<Result<SavingAccountDto>> GetByAccountNumberAsync(string accountNumber);
    // Task<Result<bool>> AccountNumberExistsAsync(string accountNumber);
    
    Task<PaginatedData<SavingAccountDto>> GetSavingAccountPagedAsync(int page, int pageSize, string? identityCardNumber = null, bool? isActive = null, string type = null);
    
    Task<Result<bool>> HasSufficientBalanceAsync(string accountNumber, decimal amount);
    Task<Result<SavingAccountDto>> GetMainAccountByUserIdAsync(string userId);
    
    Task<Result> UpdateBalanceAsync(string accountNumber, decimal amount);
    Task<Result> DepositToAccountAsync(string accountNumber, decimal amount);
    Task<Result> WithdrawFromAccount(string accountNumber, decimal amount);
    Task<Result> CancelAccountAsync(string accountNumber, string adminWhoCanceled);
    Task<Result<PaginatedData<SavingAccountDto>>> GetSavingAccountsForApiAsync(SavingApiAccountDto filters);
    Task<Result<SavingAccountDto>> CreateNewSavingAccountCard(string adminWhoApproved, string clientId,
        decimal initialAmount, bool isPrincipal = false);

    Task<List<SavingAccountDto>> GetByUserIdAsync(string userId);
    Task<Result> TransferAsync(string fromAccount, string toAccount, decimal amount);

}