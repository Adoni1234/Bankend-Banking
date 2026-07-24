using ArtemisBanking.Core.Application.Dtos.LoanInstallment;
using ArtemisBanking.Core.Domain.Entities;

namespace ArtemisBanking.Core.Application.Interfaces;

public interface ILoanInstallmentService : IGenericService<int, LoanInstallmentDto>
{
    //Task<Result<List<LoanInstallmentDto>>> GetByLoanIdAsync(string loanId);
    //Task<Result<decimal>> ProcessPaymentAsync(int loanId, decimal amount, string accountNumber);
    //Task<Result> MarkOverdueInstallmentsAsync();
    Task<Result<List<LoanInstallmentDto>>> GetInstallmentsOfLoan(string loanId);
}