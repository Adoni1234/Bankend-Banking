using ArtemisBanking.Core.Application.Dtos.Loan;

namespace ArtemisBanking.Core.Application.Interfaces;

public interface IRiskService
{
    Task<decimal> GetSystemAverageClientDebtAsync();
    Task<decimal> CalculateClientTotalDebt(string userId);
    Task<decimal> CalculateClientDebtOfAllLoansAsync(string clientId);
    Task<decimal> CalculateClientDebtOfAllCreditCardsAsync(string clientId);
    decimal CalculateTotalLoanDebt(decimal capital, decimal anualRate, int termMonths);
    Task<Result<List<ClientsWithDebtDto>>> GetDebtOfTheseUsers(List<string> usersIds, string? identityCardNumber = null);
}