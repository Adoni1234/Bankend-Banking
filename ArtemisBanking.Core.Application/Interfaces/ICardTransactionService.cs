using ArtemisBanking.Core.Application.Dtos.Beneficiary;
using ArtemisBanking.Core.Application.Dtos.CardTransaction;
using ArtemisBanking.Core.Application.Dtos.CreditCard;

namespace ArtemisBanking.Core.Application.Interfaces;

public interface ICardTransactionService : IGenericService<int, CardTransactionDto>
{
    Task<Result<CreditCardDto>> GetByCreditCardIdAsync(string cardNumber);
    Task<Result<List<CardTransactionDto>>> GetByCommerceIdAsync(int commerceId, int page, int pageSize);
    Task<Result<CardTransactionDto>> ProcessCardTransactionAsync(ProcessCardTransactionDto dto, int commerceId);
    Task<Result> ProcessPaymentAsync(ProcessCardTransactionDto dto, int commerceId);
}