using ArtemisBanking.Core.Domain.Entities;

namespace ArtemisBanking.Core.Domain.Interfaces;

public interface ICreditCardRepository : IGenericRepository<string, CreditCard>
{
    Task<CreditCard?> GetByCardNumberAsync(string cardNumber);
    Task<bool> CardExistsAndIsActiveAsync(string cardNumber);
    
    Task CancelCreditCardAsync(string creditCardNumber);
}