using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using ArtemisBanking.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories;

public class CreditCardRepository : GenericRepository<string, CreditCard>, ICreditCardRepository
{
    private readonly IIdentifierService _identifierService;
    
    public CreditCardRepository(ArtemisContext context, IIdentifierService identifierService) : base(context)
    {
        _identifierService = identifierService;
    }

    public override async Task<CreditCard> AddAsync(CreditCard entity)
    {
        entity.CardNumber = await _identifierService.GenerateCreditCardNumber();
        return await base.AddAsync(entity);
    }

    public async Task<CreditCard?> GetByCardNumberAsync(string cardNumber)
    {
        // El cardNumber es el Id (PK) de CreditCard
        return await GetByIdAsync(cardNumber);
    }

    public async Task<bool> CardExistsAndIsActiveAsync(string cardNumber)
    {
        var card = await GetByIdAsync(cardNumber);
        return card != null && card.IsActive;
    }

    public async Task CancelCreditCardAsync(string creditCardNumber)
    {
        var creditCard = await GetByIdAsync(creditCardNumber);
        if (creditCard != null)
        {
            creditCard.IsActive = false;
            await UpdateAsync(creditCard.CardNumber,  creditCard);
        }
    }
}