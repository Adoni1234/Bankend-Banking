using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Shared.Services;

public class DebtMonitorService : IDebtMonitorService
{
    private readonly ILoanInstallmentRepository  _loanInstallmentRepository;
    private readonly ILoanRepository _loanRepository;

    public DebtMonitorService(ILoanInstallmentRepository loanInstallmentRepository, ILoanRepository loanRepository)
    {
        _loanInstallmentRepository = loanInstallmentRepository;
        _loanRepository = loanRepository;
    }

    public async Task ChekcForDueInstallments()
    {
        // Las cuotas menores al dia de hoy que no esten marcadas como pagadas o como vencidas (Due)
        var loanInstallments =  await _loanInstallmentRepository.GetAllQueryable()
            .Where(inst => inst.PaymentDay < DateTime.Now)
            .Where(inst => inst.IsPaid == false)
            .Where(inst => inst.IsDue == false)
            .ToListAsync();
        
        loanInstallments.ForEach(inst => inst.IsDue = true);
        await _loanInstallmentRepository.SaveChangesAsync();
        
        var LoansIds = loanInstallments.Select(inst => inst.LoanId).Distinct().ToList();

        await _loanRepository.GetAllQueryable().Where(loan => LoansIds.Contains(loan.Id))
            .ExecuteUpdateAsync(update => update.SetProperty(inst => inst.IsDue, true));

    }
}