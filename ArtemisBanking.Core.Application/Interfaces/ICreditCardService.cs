using ArtemisBanking.Core.Application.Dtos.CreditCard;
using ArtemisBanking.Core.Application.Dtos.Loan;

namespace ArtemisBanking.Core.Application.Interfaces;

public interface ICreditCardService : IGenericService<string, CreditCardDto>
{
    // Task<Result<List<CreditCardDto>>> GetByUserIdAsync(string userId, bool? isActive = null);
    // Task<Result<bool>> ValidateCardAsync(string cardNumber, string month, string year, string cvc);
    //Task<Result> ProcessPaymentAsync(string creditCardNumber, decimal amount, string accountNumber);
    //Task<Result<decimal>> GetAvailableCreditAsync(int creditCardId);
    //Task<Result> ProcessCashAdvanceAsync(CashAdvanceDto dto);
    
    Task<Result> UpdateLimitAsync(string creditCardNumber, decimal newLimit);
    Task<Result> CancelCardAsync(string creditCardNumber); 
    Task<PaginatedData<CreditCardDto>> GetCreditCardsPagedAsync(int page, int pageSize, string? identityCardNumber = null, bool? isActive = null);
    Task<Result<CreditCardDto>> CreateNewCreditCard(string adminWhoApproved, string clientId, decimal creditLimit);
    Task<Result<decimal>> CalculateDebtOfThisCreditCardAsync(string creditCardNumber);
    
    Task<Result> UpdateBalanceAsync(string cardNumber, decimal amount);
    // cuando se paga una tarjeta, se reduce el balance de la misma
    Task<Result> ReduceBalance(string cardNumber, decimal amount);
    
    // Cuando se usa una tarjeta, se aumenta el balance de la misma.
    Task<Result> IncreaseBalance(string accountNumber, decimal amount);
    Task<List<CreditCardDto>> GetByUserIdAsync(string userId);


}