using ArtemisBanking.Core.Domain.Entities;

namespace ArtemisBanking.Core.Domain.Interfaces;

public interface ITransactionRepository : IGenericRepository<int, Transaction>
{
    Task<List<Transaction>> GetByAccountNumberAsync(string accountNumber);
    Task<List<Transaction>> GetByTellerIdAsync(string tellerId);
    Task<List<Transaction>> GetByTellerIdAndDateRangeAsync(string tellerId, DateTime startDate, DateTime endDate);
    Task<int> GetTotalTransactionsByTellerAsync(string tellerId);
    Task<int> GetTodayTransactionsByTellerAsync(string tellerId);
    Task<int> GetTotalDepositsByTellerAsync(string tellerId);
    Task<int> GetTodayDepositsByTellerAsync(string tellerId);
    Task<int> GetTotalWithdrawalsByTellerAsync(string tellerId);
    Task<int> GetTodayWithdrawalsByTellerAsync(string tellerId);
    Task<int> GetTotalPaymentsByTellerAsync(string tellerId);
    Task<int> GetTodayPaymentsByTellerAsync(string tellerId);
}