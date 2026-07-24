using ArtemisBanking.Core.Application.Dtos.Dashboard;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Core.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ISavingAccountRepository _savingAccountRepository;
    private readonly IBaseAccountService _accountService;
    private readonly ILoanRepository _loanRepository;
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly IRiskService _riskService;

    public DashboardService(
        ITransactionRepository transactionRepository,
        ISavingAccountRepository savingAccountRepository,
        IBaseAccountService accountServiceForWebApp,
        ILoanRepository loanRepository,
        ICreditCardRepository creditCardRepository,
        IRiskService riskService)
    {
        _transactionRepository = transactionRepository;
        _savingAccountRepository = savingAccountRepository;
        _accountService = accountServiceForWebApp;
        _loanRepository = loanRepository;
        _creditCardRepository = creditCardRepository;
        _riskService = riskService;
    }

    public async Task<DashboardAdminDto> GetAdminDashboard()
    {
        var today = DateTime.Today;

        var totalTransactions = await _transactionRepository
            .GetAllQueryable()
            .AsNoTracking()
            .CountAsync();

        var todayTransactions = await _transactionRepository
            .GetAllQueryable()
            .AsNoTracking()
            .CountAsync(t => t.Date.Date == today);

        var totalPayments = await _transactionRepository
            .GetAllQueryable()
            .AsNoTracking()
            .CountAsync(t =>
                t.SubType == TransactionSubType.LoanPayment ||
                t.SubType == TransactionSubType.CreditCardPayment);

        var todayPayments = await _transactionRepository
            .GetAllQueryable()
            .AsNoTracking()
            .CountAsync(t =>
                t.Date.Date == today &&
                (t.SubType == TransactionSubType.LoanPayment ||
                 t.SubType == TransactionSubType.CreditCardPayment));

        var activeClients = await _accountService.CountUsers(Roles.Client, true);
        var inactiveClients = await _accountService.CountUsers(Roles.Client, false);

        var totalProducts =
              await _savingAccountRepository.GetAllQueryable().CountAsync()
            + await _loanRepository.GetAllQueryable().CountAsync()
            + await _creditCardRepository.GetAllQueryable().CountAsync();

        var activeLoans = await _loanRepository
            .GetAllQueryable()
            .CountAsync(l => !l.Completed);

        var activeCreditCards = await _creditCardRepository
            .GetAllQueryable()
            .CountAsync(c => c.IsActive);

        var activeSavingsAccounts = await _savingAccountRepository
            .GetAllQueryable()
            .CountAsync(sa => sa.IsActive);

        var avgDebt = await _riskService.GetSystemAverageClientDebtAsync();

        return new DashboardAdminDto
        {
            TotalTransactions = totalTransactions,
            TodayTransactions = todayTransactions,

            TotalPayments = totalPayments,
            TodayPayments = todayPayments,

            ActiveClients = activeClients,
            InactiveClients = inactiveClients,

            TotalProducts = totalProducts,

            ActiveLoans = activeLoans,
            ActiveCreditCards = activeCreditCards,
            ActiveSavingsAccounts = activeSavingsAccounts,

            AverageClientDebt =  avgDebt
        };
    }
}
