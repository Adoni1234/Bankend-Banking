using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using ArtemisBanking.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories;

public class TransactionRepository : GenericRepository<int, Transaction>, ITransactionRepository
{
    public TransactionRepository(ArtemisContext context) : base(context)
    {
    }

    public async Task<List<Transaction>> GetByAccountNumberAsync(string accountNumber)
    {
        return await Context.Set<Transaction>()
            .Where(t => t.AccountNumber == accountNumber)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetByTellerIdAsync(string tellerId)
    {
       return await Context.Set<Transaction>()
            .Where(t => t.CreatedById == tellerId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetByTellerIdAndDateRangeAsync(string tellerId, DateTime startDate, DateTime endDate)
    {
        return await Context.Set<Transaction>()
            .Where(t => t.CreatedById == tellerId
                     && t.Date >= startDate 
                     && t.Date <= endDate)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<int> GetTotalTransactionsByTellerAsync(string tellerId)
    {
        return await Context.Set<Transaction>()
            .CountAsync(t => t.CreatedById == tellerId);
    }

    public async Task<int> GetTodayTransactionsByTellerAsync(string tellerId)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        
        return await Context.Set<Transaction>()
            .CountAsync(t => t.CreatedById == tellerId && t.Date >= today && t.Date < tomorrow);
    }

    public async Task<int> GetTotalDepositsByTellerAsync(string tellerId)
    {
        // Los depósitos son transacciones de tipo Credit donde el Origin contiene el TellerId
        return await Context.Set<Transaction>()
            .Where(t => t.CreatedById == tellerId)
            .Where(t => t.SubType == TransactionSubType.Deposit)
            .CountAsync();

    }

    public async Task<int> GetTodayDepositsByTellerAsync(string tellerId)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await Context.Set<Transaction>()
            .Where(t => t.SubType == TransactionSubType.Deposit)
            .Where(t => t.CreatedById == tellerId)
            .Where(t => t.Date >= today && t.Date < tomorrow)
            .CountAsync();
    }

    public async Task<int> GetTotalWithdrawalsByTellerAsync(string tellerId)
    {
        // Los retiros son transacciones de tipo Debit
        return await Context.Set<Transaction>()
            .Where(t => t.CreatedById == tellerId)
            .Where(t => t.SubType == TransactionSubType.Withdrawal)
            .CountAsync();

    }

    public async Task<int> GetTodayWithdrawalsByTellerAsync(string tellerId)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await Context.Set<Transaction>()
            .Where(t => t.SubType == TransactionSubType.Withdrawal)
            .Where(t => t.CreatedById == tellerId)
            .Where(t => t.Date >= today && t.Date < tomorrow)
            .CountAsync();
    }

    public async Task<int> GetTotalPaymentsByTellerAsync(string tellerId)
    {
        // Los pagos incluyen pagos de tarjeta de crédito y préstamos
        // Esto es una aproximación - puedes ajustar según tu lógica de negocio
        return await Context.Set<Transaction>()
            .Where(t => t.CreatedById == tellerId)
            .Where(t => t.SubType == TransactionSubType.CreditCardPayment ||
                        t.SubType == TransactionSubType.LoanPayment)
            .CountAsync();
    }

    public async Task<int> GetTodayPaymentsByTellerAsync(string tellerId)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await Context.Set<Transaction>()
            .Where(t => t.SubType == TransactionSubType.CreditCardPayment ||
                        t.SubType == TransactionSubType.LoanPayment)
            .Where(t => t.CreatedById == tellerId)
            .Where(t => t.Date >= today && t.Date < tomorrow)
            .CountAsync();
    }
}