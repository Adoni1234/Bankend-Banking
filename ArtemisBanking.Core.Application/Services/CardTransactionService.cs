using ArtemisBanking.Core.Application.Dtos.Beneficiary;
using ArtemisBanking.Core.Application.Dtos.CardTransaction;
using ArtemisBanking.Core.Application.Dtos.Commerce;
using ArtemisBanking.Core.Application.Dtos.CreditCard;
using ArtemisBanking.Core.Application.Dtos.Email;
using ArtemisBanking.Core.Application.Enums;
using ArtemisBanking.Core.Application.Helpers;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Core.Application.Services;

public class CardTransactionService : GenericServices<int, CardTransaction, CardTransactionDto>, ICardTransactionService 
{
    private readonly ICreditCardRepository _cardRepository;
    private readonly ICardTransactionRepository _cardTransactionRepository;
    private readonly IBaseAccountService _accountServiceForWebApp;
    private readonly ICreditCardService _creditCardService;
    private readonly IEmailService _emailService;
    private readonly ICommerceService _commerceService;
    private readonly ISavingAccountService _savingAccountService;
    private readonly IMapper _mapper;
    
    public CardTransactionService(
        ICardTransactionRepository repository, 
        IMapper mapper, 
        ICreditCardRepository cardRepository, 
        IBaseAccountService accountServiceForWebApp, 
        ICreditCardService creditCardService,
        IEmailService emailService,
        ICommerceService commerceService,
        ISavingAccountService savingAccountService) : base(repository, mapper)
    {
        _cardTransactionRepository = repository;
        _mapper = mapper;
        _cardRepository = cardRepository;
        _accountServiceForWebApp = accountServiceForWebApp;
        _creditCardService = creditCardService;
        _emailService = emailService;
        _commerceService = commerceService;
        _savingAccountService = savingAccountService;
    }

    public async Task<Result<CreditCardDto>> GetByCreditCardIdAsync(string cardNumber)
    {
        var card = await _cardRepository.GetAllQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);

        if (card == null)
            return Result<CreditCardDto>.Fail("Card not found");

        var client = await _accountServiceForWebApp.GetUserById(card.ClientId);

        if (client == null)
            return Result<CreditCardDto>.Fail("No se encontro al usuario");
            


        var transactions = await _cardTransactionRepository.GetAllQueryable()
            .AsNoTracking()
            .Where(ct => ct.CreditCardNumber == cardNumber)
            .OrderByDescending(ct => ct.Date)
            .ToListAsync();

        var cardDto = _mapper.Map<CreditCardDto>(card);
        cardDto.CardTransactions = _mapper.Map<List<CardTransactionDto>>(transactions);
        cardDto.Client = client;

        return Result<CreditCardDto>.Ok(cardDto);
    }


    public async Task<Result<List<CardTransactionDto>>> GetByCommerceIdAsync(int commerceId, int page, int pageSize)
    {
        var query = _cardTransactionRepository.GetAllQueryable()
            .AsNoTracking()
            .Where(ct => ct.CommerceId == commerceId)
            .OrderByDescending(ct => ct.Date)
            .Select(ct => _mapper.Map<CardTransactionDto>(ct));

        var transactions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return Result<List<CardTransactionDto>>.Ok(transactions);
    }

    public async Task<Result<CardTransactionDto>> ProcessCardTransactionAsync(ProcessCardTransactionDto dto, int commerceId)
    {
        try
        {
            // Validar que la tarjeta existe
            var creditCard = await _cardRepository.GetByCardNumberAsync(dto.CreditCardNumber);
            if (creditCard == null)
            {
                return Result<CardTransactionDto>.Fail("La tarjeta de crédito especificada no existe.");
            }

            if (!creditCard.IsActive)
            {
                return Result<CardTransactionDto>.Fail("La tarjeta de crédito especificada está inactiva.");
            }

            // Validar fecha de expiración
            var currentDate = DateTime.Now;
            if (creditCard.ExpirationYear < currentDate.Year || 
                (creditCard.ExpirationYear == currentDate.Year && creditCard.ExpirationMonth < currentDate.Month))
            {
                return Result<CardTransactionDto>.Fail("La tarjeta de crédito ha expirado.");
            }

            // Validar mes y año de expiración
            if (creditCard.ExpirationMonth != dto.ExpirationMonth || creditCard.ExpirationYear != dto.ExpirationYear)
            {
                return Result<CardTransactionDto>.Fail("La fecha de expiración proporcionada no coincide con la tarjeta.");
            }

            // Validar CVC (hashear el CVC proporcionado y comparar con el almacenado)
            var providedCvcHash = CvcEncryptation.ComputeSha256Hash(dto.Cvc);
            if (creditCard.CvcHashed != providedCvcHash)
            {
                return Result<CardTransactionDto>.Fail("El CVC proporcionado es incorrecto.");
            }

            // Validar que hay crédito disponible
            var availableCredit = creditCard.CreditLimit - creditCard.Balance;
            if (availableCredit < dto.Amount)
            {
                return Result<CardTransactionDto>.Fail("La tarjeta no tiene suficiente crédito disponible para esta transacción.");
            }

            // Actualizar el balance de la tarjeta (aumentar deuda)
            var newBalance = creditCard.Balance + dto.Amount;
            creditCard.Balance = newBalance;
            
            // Actualizar la tarjeta directamente en el repositorio
            await _cardRepository.UpdateAsync(creditCard.CardNumber, creditCard);

            // Crear la transacción de tarjeta
            var cardTransaction = new CardTransaction
            {
                CreditCardNumber = dto.CreditCardNumber,
                Amount = dto.Amount,
                CommerceId = commerceId,
                IsCashAdvance = dto.IsCashAdvance,
                Date = DateTime.Now,
                Status = CreditCardTransactionStatus.Approved
            };

            var savedTransaction = await _cardTransactionRepository.AddAsync(cardTransaction);
            var transactionDto = _mapper.Map<CardTransactionDto>(savedTransaction);

            return Result<CardTransactionDto>.Ok(transactionDto);
        }
        catch (Exception ex)
        {
            return Result<CardTransactionDto>.Fail($"Error al procesar la transacción de tarjeta: {ex.Message}");
        }
    }

    public async Task<Result> ProcessPaymentAsync(ProcessCardTransactionDto dto, int commerceId)
    {
        try
        {
            // Validar que el comercio existe
            var commerceResult = await _commerceService.GetByIdAsync(commerceId);
            if (commerceResult.IsFailure)
            {
                return Result.Fail("El comercio especificado no existe.");
            }

            var commerce = commerceResult.Value!;
            if (!commerce.IsActive)
            {
                return Result.Fail("El comercio especificado está inactivo.");
            }

            // Validar que la tarjeta existe
            var creditCard = await _cardRepository.GetByCardNumberAsync(dto.CreditCardNumber);
            if (creditCard == null)
            {
                return Result.Fail("La tarjeta de crédito especificada no existe.");
            }

            if (!creditCard.IsActive)
            {
                return Result.Fail("La tarjeta de crédito especificada está inactiva.");
            }

            // Validar fecha de expiración
            var currentDate = DateTime.Now;
            if (creditCard.ExpirationYear < currentDate.Year || 
                (creditCard.ExpirationYear == currentDate.Year && creditCard.ExpirationMonth < currentDate.Month))
            {
                return Result.Fail("La tarjeta de crédito ha expirado.");
            }

            // Validar mes y año de expiración
            if (creditCard.ExpirationMonth != dto.ExpirationMonth || creditCard.ExpirationYear != dto.ExpirationYear)
            {
                return Result.Fail("La fecha de expiración proporcionada no coincide con la tarjeta.");
            }

            // Validar CVC (hashear el CVC proporcionado y comparar con el almacenado)
            var providedCvcHash = CvcEncryptation.ComputeSha256Hash(dto.Cvc);
            if (creditCard.CvcHashed != providedCvcHash)
            {
                return Result.Fail("El CVC proporcionado es incorrecto.");
            }

            // Validar que hay crédito disponible
            var availableCredit = creditCard.CreditLimit - creditCard.Balance;
            if (availableCredit < dto.Amount)
            {
                return Result.Fail("La tarjeta no tiene suficiente crédito disponible para esta transacción.");
            }

            // Actualizar el balance de la tarjeta (aumentar deuda)
            var newBalance = creditCard.Balance + dto.Amount;
            creditCard.Balance = newBalance;
            await _cardRepository.UpdateAsync(creditCard.CardNumber, creditCard);

            // Crear la transacción de tarjeta
            var cardTransaction = new CardTransaction
            {
                CreditCardNumber = dto.CreditCardNumber,
                Amount = dto.Amount,
                CommerceId = commerceId,
                IsCashAdvance = dto.IsCashAdvance,
                Date = DateTime.Now,
                Status = CreditCardTransactionStatus.Approved
            };

            await _cardTransactionRepository.AddAsync(cardTransaction);

            // Obtener el usuario del comercio para acreditar el monto a su cuenta principal
            var commerceUsersResult = await _accountServiceForWebApp.GetAllUserOfRole(Roles.Commerce, isActive: true);
            var commerceUser = commerceUsersResult.Value?.FirstOrDefault(u => u.CommerceId == commerceId);
            
            if (commerceUser != null)
            {
                // Obtener la cuenta principal del comercio
                var mainAccountResult = await _savingAccountService.GetMainAccountByUserIdAsync(commerceUser.Id);
                if (mainAccountResult.IsSuccess && mainAccountResult.Value != null)
                {
                    // Acreditar el monto a la cuenta de ahorro principal del comercio
                    await _savingAccountService.DepositToAccountAsync(mainAccountResult.Value.Id, dto.Amount);
                }
            }

            // Obtener el cliente dueño de la tarjeta
            var cardOwner = await _accountServiceForWebApp.GetUserById(creditCard.ClientId);
            if (cardOwner == null)
            {
                return Result.Fail("No se encontró al cliente propietario de la tarjeta.");
            }

            // Obtener el usuario del comercio para enviar el email
            if (commerceUser == null)
            {
                commerceUsersResult = await _accountServiceForWebApp.GetAllUserOfRole(Roles.Commerce, isActive: false);
                commerceUser = commerceUsersResult.Value?.FirstOrDefault(u => u.CommerceId == commerceId);
            }

            var transactionDate = DateTime.Now;
            var cardLast4 = dto.CreditCardNumber.Substring(dto.CreditCardNumber.Length - 4);

            // Enviar email al cliente
            await _emailService.SendAsync(new EmailRequestDto
            {
                To = cardOwner.Email,
                Subject = $"Consumo realizado con la tarjeta {cardLast4}",
                HtmlBody = $@"
                    <html>
                    <body>
                        <h2>Consumo realizado con su tarjeta de crédito</h2>
                        <p>Se ha realizado un consumo con su tarjeta de crédito.</p>
                        <ul>
                            <li><strong>Monto:</strong> {dto.Amount:C}</li>
                            <li><strong>Tarjeta:</strong> ****{cardLast4}</li>
                            <li><strong>Comercio:</strong> {commerce.Name}</li>
                            <li><strong>Fecha:</strong> {transactionDate:dd/MM/yyyy}</li>
                            <li><strong>Hora:</strong> {transactionDate:HH:mm:ss}</li>
                        </ul>
                    </body>
                    </html>"
            });

            // Enviar email al comercio (si tiene usuario asociado)
            if (commerceUser != null)
            {
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = commerceUser.Email,
                    Subject = $"Pago recibido a través de tarjeta {cardLast4}",
                    HtmlBody = $@"
                        <html>
                        <body>
                            <h2>Pago recibido a través de tarjeta de crédito</h2>
                            <p>Ha recibido un nuevo pago a través de una tarjeta de crédito.</p>
                            <ul>
                                <li><strong>Monto recibido:</strong> {dto.Amount:C}</li>
                                <li><strong>Tarjeta:</strong> ****{cardLast4}</li>
                                <li><strong>Comercio:</strong> {commerce.Name}</li>
                                <li><strong>Fecha:</strong> {transactionDate:dd/MM/yyyy}</li>
                                <li><strong>Hora:</strong> {transactionDate:HH:mm:ss}</li>
                            </ul>
                        </body>
                        </html>"
                });
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error al procesar el pago: {ex.Message}");
        }
    }
}