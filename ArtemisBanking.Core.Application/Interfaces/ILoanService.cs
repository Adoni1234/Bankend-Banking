using ArtemisBanking.Core.Application.Dtos.Loan;
using ArtemisBanking.Core.Application.Dtos.LoanInstallment;
using ArtemisBanking.Core.Application.Dtos.User;

namespace ArtemisBanking.Core.Application.Interfaces;

public interface ILoanService : IGenericService<string, LoanDto>
{
    // Task<Result<List<LoanDto>>> GetByUserIdAsync(string userId, bool? isActive = null);
    Task<bool> UserHasActiveLoanAsync(string userId);


    Task<Result<List<ClientsWithDebtDto>>> GetClientsWithoutActiveLoan(string? identityCardNumber = null);
    Task<PaginatedData<LoanSummaryDto>> GetLoansPagedAsync(int page, int pageSize, string? identityCardNumber = null, bool? isCompleted = null);
    Task<Result<LoanDto>> GetAmortizationTableAsync(string loanId);
    Task<Result<LoanDto>> UpdateInterestRateAsync(string loanId, decimal newRate);
    //Task<Result<bool>> IsHighRiskClientAsync(string userId, decimal newLoanAmount);
    Task<Result<decimal>> PayAsync(string loanId, decimal amount);
    Task<List<LoanDto>> GetByUserIdAsync(string userId);

}