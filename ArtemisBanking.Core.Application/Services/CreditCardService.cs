using ArtemisBanking.Core.Application.Dtos.CreditCard;
using ArtemisBanking.Core.Application.Dtos.Email;
using ArtemisBanking.Core.Application.Dtos.User;
using ArtemisBanking.Core.Application.Enums;
using ArtemisBanking.Core.Application.Helpers;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Core.Application.Services;

public class CreditCardService : GenericServices<string, CreditCard, CreditCardDto>, ICreditCardService
{
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICardTransactionRepository _cardTransactionRepository;
    
    private readonly IEmailService _emailService;
    private readonly IBaseAccountService _accountServiceForWebApp;
    private readonly IRiskService _riskService;
    private readonly IMapper _mapper;

    public CreditCardService(ICreditCardRepository repository, IMapper mapper,
        IBaseAccountService accountServiceForWebApp, IRiskService riskService, IEmailService emailService, ITransactionRepository transactionRepository, ICardTransactionRepository cardTransactionRepository) : base(repository, mapper)
    {
        _creditCardRepository = repository;
        _mapper = mapper;
        _accountServiceForWebApp = accountServiceForWebApp;
        this._riskService = riskService;
        _emailService = emailService;
        _transactionRepository = transactionRepository;
        _cardTransactionRepository = cardTransactionRepository;
    }

    public override async Task<Result<CreditCardDto>> AddAsync(CreditCardDto dtoModel)
    {
        var client = await _accountServiceForWebApp.GetUserById(dtoModel.ClientId);
        if (client is null)
        {
            return Result<CreditCardDto>.Fail("no se encontro al usuario");
        }
        
        var userWhoApproved = await _accountServiceForWebApp.GetUserById(dtoModel.ApprovedByUserId);
        if (userWhoApproved is null)
        {
            return Result<CreditCardDto>.Fail("no se encontro al usuario");
        }

        if (userWhoApproved.Role != nameof(Roles.Admin))
        {

            return Result<CreditCardDto>.Fail("No estas autorizado para asignar tarjetas de credito");
        }

        return await base.AddAsync(dtoModel);
    }

    public async Task<PaginatedData<CreditCardDto>> GetCreditCardsPagedAsync(int page, int pageSize,
        string? identityCardNumber = null, bool? isActive = null)
    {
        UserDto? client = null;
        if (!string.IsNullOrEmpty(identityCardNumber))
        {
            var clientResult = await _accountServiceForWebApp.GetByIdentityCardNumber(identityCardNumber);

            if (clientResult.IsFailure || clientResult.Value is null)
                return new PaginatedData<CreditCardDto>([], 0, page, pageSize);

            client = clientResult.Value;
        }

        var query = _creditCardRepository.GetAllQueryable().AsNoTracking();

        if (client is not null)
            query = query.Where(l => l.ClientId == client.Id);

        if (isActive.HasValue)
            query = query.Where(l => l.IsActive == isActive.Value);

        var usersIds = await query.Select(l => l.ClientId).Distinct().ToListAsync();
        var usersResult = await _accountServiceForWebApp.GetUsersByIds(usersIds);
        var usersDict = usersResult.Value!.ToDictionary(user => user.Id, user => user);

        var items = await query
            .OrderByDescending(l => l.IsActive)
            .ThenByDescending(l => l.CreatedAt)
            .Select(c => new CreditCardDto
            {
                ClientId = c.ClientId,
                Client = usersDict[c.ClientId],
                CardNumber = c.CardNumber,
                ApprovedByUserId = c.ApprovedByUserId,
                CreditLimit = c.CreditLimit,
                Balance = c.Balance,
                CvcHashed = c.CvcHashed,
                ExpirationMonth = c.ExpirationMonth,
                ExpirationYear = c.ExpirationYear,
                CreatedAt = c.CreatedAt,
                IsActive = c.IsActive,
            })
            .ToListAsync();

        var data = PaginatedData<CreditCardDto>.Create(items, page, pageSize);
        return data;
    }

    public Task<Result<CreditCardDto>> CreateNewCreditCard(string adminWhoApproved ,string clientId, decimal creditLimit)
    {
        var expirationDate = DateTime.Today.AddYears(3);
        var card = new CreditCardDto()
        {
            CardNumber = "",
            ClientId = clientId,
            ApprovedByUserId = adminWhoApproved,
            CreditLimit = creditLimit,
            Balance = 0,
            CvcHashed = GenerateEncryptedCvc(),
            ExpirationMonth = expirationDate.Month,
            ExpirationYear = expirationDate.Year,
            CreatedAt = DateTime.Today,
            IsActive = true
        };
        return AddAsync(card);
    }

    
    public async Task<Result<decimal>> CalculateDebtOfThisCreditCardAsync(string creditCardNumber)
    {
        //TODO probar si con el balance de la tarjeta da los mismos resultados
        var cardExists = await _creditCardRepository.GetAllQueryable()
            .AnyAsync(c => c.CardNumber == creditCardNumber && c.IsActive);
    
        if (!cardExists)
        {
            return Result<decimal>.Fail("No se encontró una tarjeta activa con este número");
        }

        var totalConsumos = await _cardTransactionRepository.GetAllQueryable()
            .Where(ct => ct.CreditCardNumber == creditCardNumber && 
                         ct.Status == CreditCardTransactionStatus.Approved)
            .SumAsync(ct => ct.Amount); 

        var totalPagos = await _transactionRepository.GetAllQueryable()
            .Where(t => t.Beneficiary == creditCardNumber &&
                        t.SubType == TransactionSubType.CreditCardPayment && 
                        t.Status == TransactionStatus.Approved)
            .SumAsync(t => t.Amount); 

        var totalDebt = totalConsumos - totalPagos;
    
        return Result<decimal>.Ok(totalDebt);
    }

    public async Task<Result> UpdateBalanceAsync(string cardNumber, decimal amount)
    {
        var card = await _creditCardRepository.GetByIdAsync(cardNumber);
        if (card is null)
        {
            return Result.Fail("No se encontró una tarjeta con ese numero");
        }

        if (!card.IsActive)
        {
            return Result.Fail("Esta tarjeta se encuentra cancelada");
        }

        card.Balance = amount;
        await _creditCardRepository.UpdateAsync(card.CardNumber, card);
        return Result.Ok();
    }

    
    // cuando se paga una tarjeta, se reduce el balance de la misma
    public async Task<Result> ReduceBalance(string cardNumber, decimal amount)
    {
        
        var card = await _creditCardRepository.GetByIdAsync(cardNumber);
        if (card is null)
        {
            return Result.Fail("No se encontró una tarjeta con ese numero");
        }

        if (!card.IsActive)
        {
            return Result.Fail("Esta tarjeta se encuentra cancelada");
        }

        card.Balance += amount;
        await _creditCardRepository.UpdateAsync(card.CardNumber, card);
        return Result.Ok();
    }

    
    // Cuando se usa una tarjeta, se aumenta el balance de la misma.
    public async Task<Result> IncreaseBalance(string cardNumber, decimal amount)
    {
        
        var card = await _creditCardRepository.GetByIdAsync(cardNumber);
        if (card is null)
        {
            return Result.Fail("No se encontró una tarjeta con ese numero");
        }

        if (!card.IsActive)
        {
            return Result.Fail("Esta tarjeta se encuentra cancelada");
        }

        card.Balance -= amount;
        await _creditCardRepository.UpdateAsync(card.CardNumber, card);
        return Result.Ok();
    }

    public async Task<Result> UpdateLimitAsync(string creditCardNumber, decimal newLimit)
    {
        var card = await _creditCardRepository.GetByIdAsync(creditCardNumber);
        if (card is null)
        {
            return Result.Fail("No se encontro una tarjeta con ese numero");
        }

        if (!card.IsActive)
        {
            return Result.Fail("Esta tarjeta se encuentra cancelada");
        }

        var cardDebtResult  = await CalculateDebtOfThisCreditCardAsync(creditCardNumber);
        if (cardDebtResult.IsFailure)
        {
            return Result.Fail(cardDebtResult.GeneralError!);
        }
        
        if (newLimit < cardDebtResult.Value)
        {
            return Result.Fail("El nuevo limite de esta tarjeta es menor a la deuda de la misma. No se puede cambiar");
        }

        card.CreditLimit = newLimit;
        await _creditCardRepository.UpdateAsync(card.CardNumber, card);
        
        // para el Email
        
        var client = await _accountServiceForWebApp.GetUserById(card.ClientId);
        if (client is null)
        {
            return Result.Fail("No se encontro al cliente");
        }
        
        await _emailService.SendTemplateEmailAsync(new EmailTemplateDataDto
        {
            Type = EmailType.CreditCardLimitUpdated,
            To = client.Email,
            Variables =
            {
                ["CardNumberFourDigits"] = card.CardNumber.Substring(card.CardNumber.Length - 4),
                ["NewLimit"] = newLimit.ToString("N2"),
                ["Date"] = DateTime.Now.ToShortDateString(),
            }
        });
        return Result.Ok();
        
    }

    public async Task<Result> CancelCardAsync(string creditCardNumber)
    {
        var card = await _creditCardRepository.GetByIdAsync(creditCardNumber);
        if (card is null)
        {
            return Result.Fail("No se encontró una tarjeta con ese numero");
        }
        
        var cardDebtResult  = await CalculateDebtOfThisCreditCardAsync(creditCardNumber);
        if (cardDebtResult.IsFailure)
        {
            return Result.Fail(cardDebtResult.GeneralError!);
        }
        
        if (cardDebtResult.Value > 0)
        {
            return Result.Fail("Para cancelar esta tarjeta, el cliente debe saldar la " +
                               "totalidad de la deuda pendiente.");
        }
        
        card.IsActive = false;
        await _creditCardRepository.UpdateAsync(card.CardNumber, card);
        return Result.Ok();
    }
    
    private string GenerateEncryptedCvc()
    {
        var rnd = new Random();
        int sequence = rnd.Next(0, 1000); // 000–999
        var cvc = sequence.ToString("D3");
        var cvcHashed = CvcEncryptation.ComputeSha256Hash(cvc);
        return cvcHashed;
    }

    public async Task<List<CreditCardDto>> GetByUserIdAsync(string userId)
    {
        var list = await _creditCardRepository.GetAllAsync();

        return list
            .Where(c => c.ClientId == userId)
            .Select(c => _mapper.Map<CreditCardDto>(c))
            .ToList();
    }


}