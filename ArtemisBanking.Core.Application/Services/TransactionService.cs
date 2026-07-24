using ArtemisBanking.Core.Application.Dtos.CreditCard;
using ArtemisBanking.Core.Application.Dtos.Email;
using ArtemisBanking.Core.Application.Dtos.SavingAccount;
using ArtemisBanking.Core.Application.Dtos.Transaction;
using ArtemisBanking.Core.Application.Dtos.Transaction.Teller;
using ArtemisBanking.Core.Application.Dtos.Transfer;
using ArtemisBanking.Core.Application.Enums;
using ArtemisBanking.Core.Application.Helpers;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace ArtemisBanking.Core.Application.Services;

public class TransactionService : GenericServices<int, Transaction, TransactionDto>, ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ISavingAccountRepository _savingAccountRepository;
    private readonly ILoanRepository _loanRepository;
    
    private readonly IEmailService _emailService;
    private readonly IBaseAccountService _accountServiceForWebApp;
    private readonly ISavingAccountService _savingAccountService;
    private readonly ICreditCardService _creditCardService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMapper _mapper;

    public TransactionService(ITransactionRepository transactionRepository, IMapper mapper,
        ICreditCardRepository creditCardRepository, ISavingAccountRepository accountRepository,
        ILoanRepository loanRepository, IEmailService emailService, IBaseAccountService accountServiceForWebApp,
        ISavingAccountService savingAccountService, ICreditCardService creditCardService, IServiceProvider serviceProvider) : base(
        transactionRepository, mapper)
    {
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _creditCardRepository = creditCardRepository;
        _savingAccountRepository = accountRepository;
        _loanRepository = loanRepository;
        _emailService = emailService;
        _accountServiceForWebApp = accountServiceForWebApp;
        _savingAccountService = savingAccountService;
        _creditCardService = creditCardService;
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<List<TransactionDto>>> GetByAccountNumberAsync(string accountNumber)
    {
        try
        {
            var transactions = await _transactionRepository.GetByAccountNumberAsync(accountNumber);
            var dtos = _mapper.Map<List<TransactionDto>>(transactions);
            return Result<List<TransactionDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<TransactionDto>>.Fail($"Error al obtener transacciones: {ex.Message}");
        }
    }

    /*
    public async Task<Result<TransactionDto>> ProcessExpressTransferAsync(ExpressTransferDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<TransactionDto>> ProcessBeneficiaryTransferAsync(BeneficiaryTransferDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<TransactionDto>> ProcessCreditCardPaymentAsync(CreditCardPaymentDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<TransactionDto>> ProcessLoanPaymentAsync(LoanPaymentDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<TransactionDto>> ProcessAccountTransferAsync(AccountTransferDto dto)
    {
        throw new NotImplementedException();
    }
    */

    public async Task<Result<TransactionDto>> ProcessDepositAsync(DepositDto dto)
    {
        try
        {
            // Validar que la cuenta existe y está activa
            var account = await _savingAccountRepository.GetByAccountNumberAsync(dto.AccountNumber);
            if (account == null)
            {
                return Result<TransactionDto>.Fail("La cuenta especificada no existe.");
            }

            if (!account.IsActive)
            {
                return Result<TransactionDto>.Fail("La cuenta especificada está inactiva.");
            }

            // Actualizar el balance de la cuenta (aumentar)
            var newBalance = account.Balance + dto.Amount;
            var updateResult = await _savingAccountService.UpdateBalanceAsync(dto.AccountNumber, newBalance);
            if (updateResult.IsFailure)
            {
                return Result<TransactionDto>.Fail(updateResult.GeneralError ??
                                                   "Error al actualizar el balance de la cuenta.");
            }

            // Crear la transacción
            var transaction = new Transaction
            {
                Amount = dto.Amount,
                Type = TransactionType.Credit,
                Origin = "DEPOSITO", // El cajero que procesa el depósito
                Beneficiary = dto.AccountNumber, // La cuenta que recibe el depósito
                Date = DateTime.Now,
                Status = TransactionStatus.Approved,
                AccountNumber = dto.AccountNumber, // La cuenta afectada (donde se deposita)
                CreatedById = dto.TellerId, // Quien hizo la transacción (el cajero)
                SubType = TransactionSubType.Deposit // Tipo de transacción: Depósito
            };

            var savedTransaction = await _transactionRepository.AddAsync(transaction);
            var transactionDto = _mapper.Map<TransactionDto>(savedTransaction);
            
            var client = await _accountServiceForWebApp.GetUserById(account.ClientId);
            if (client == null)
            {
                return Result<TransactionDto>.Fail("No se encontró al cliente");
            }
            
            await _emailService.SendTemplateEmailAsync(new EmailTemplateDataDto()
            {
                Type = EmailType.Deposit,
                To = client.Email,
                Variables =
                {
                    ["AccountLast4"] = account.Id[^4..],
                    ["Amount"] = dto.Amount.ToString("N2"),
                    ["Date"] = DateTime.Now.ToShortDateString(),
                    ["Time"] = DateTime.Now.ToShortTimeString()
                }
            });
            

            return Result<TransactionDto>.Ok(transactionDto);
        }
        catch (Exception ex)
        {
            return Result<TransactionDto>.Fail($"Error al procesar el depósito: {ex.Message}");
        }
    }

    public async Task<Result<TransactionDto>> ProcessWithdrawalAsync(WithdrawalDto dto)
    {
        try
        {
            // Validar que la cuenta existe y está activa
            var account = await _savingAccountRepository.GetByAccountNumberAsync(dto.AccountNumber);
            if (account == null)
            {
                return Result<TransactionDto>.Fail("La cuenta especificada no existe.");
            }

            if (!account.IsActive)
            {
                return Result<TransactionDto>.Fail("La cuenta especificada está inactiva.");
            }

            // Validar que hay suficiente saldo
            if (account.Balance < dto.Amount)
            {
                return Result<TransactionDto>.Fail("La cuenta no tiene suficiente saldo para realizar este retiro.");
            }

            // Actualizar el balance de la cuenta (disminuir)
            var newBalance = account.Balance - dto.Amount;
            var updateResult = await _savingAccountService.UpdateBalanceAsync(dto.AccountNumber, newBalance);
            if (updateResult.IsFailure)
            {
                return Result<TransactionDto>.Fail(updateResult.GeneralError ??
                                                   "Error al actualizar el balance de la cuenta.");
            }

            // Crear la transacción
            var transaction = new Transaction
            {
                Amount = dto.Amount,
                Type = TransactionType.Debit,
                Origin = dto.AccountNumber, // La cuenta de donde se retira
                Beneficiary = "RETIRO", // El dinero se retira en efectivo
                Date = DateTime.Now,
                Status = TransactionStatus.Approved,
                AccountNumber = dto.AccountNumber, // La cuenta afectada (de donde se retira)
                CreatedById = dto.TellerId, // Quien hizo la transacción (el cajero)
                SubType = TransactionSubType.Withdrawal // Tipo de transacción: Retiro
            };

            var savedTransaction = await _transactionRepository.AddAsync(transaction);
            var transactionDto = _mapper.Map<TransactionDto>(savedTransaction);
            
            var client = await _accountServiceForWebApp.GetUserById(account.ClientId);
            if (client == null)
            {
                return Result<TransactionDto>.Fail("No se encontró al cliente");
            }
            
            await _emailService.SendTemplateEmailAsync(new EmailTemplateDataDto()
            {
                Type = EmailType.Withdraw,
                To = client.Email,
                Variables =
                {
                    ["AccountLast4"] = account.Id[^4..],
                    ["Amount"] = dto.Amount.ToString("N2"),
                    ["Date"] = DateTime.Now.ToShortDateString(),
                    ["Time"] = DateTime.Now.ToShortTimeString()
                }
            });

            return Result<TransactionDto>.Ok(transactionDto);
        }
        catch (Exception ex)
        {
            return Result<TransactionDto>.Fail($"Error al procesar el retiro: {ex.Message}");
        }
    }

    public async Task<Result> ProcessTellerTransactionAsync(TellerTransactionDto dto)
    {
        try
        {
            // Validar que las cuentas no sean la misma
            if (dto.SourceAccountNumber == dto.DestinationAccountNumber)
            {
                return Result<TransactionDto>.Fail("La cuenta origen y destino no pueden ser la misma.");
            }

            // Validar cuenta origen
            var sourceAccount = await _savingAccountRepository.GetByAccountNumberAsync(dto.SourceAccountNumber);
            if (sourceAccount == null)
            {
                return Result<TransactionDto>.Fail("La cuenta origen especificada no existe.");
            }

            if (!sourceAccount.IsActive)
            {
                return Result<TransactionDto>.Fail("La cuenta origen especificada está inactiva.");
            }

            // Validar que hay suficiente saldo
            if (sourceAccount.Balance < dto.Amount)
            {
                return Result<TransactionDto>.Fail(
                    "La cuenta origen no tiene suficiente saldo para realizar esta transacción.");
            }

            // Validar cuenta destino
            var destinationAccount = await _savingAccountRepository.GetByAccountNumberAsync(dto.DestinationAccountNumber);
            if (destinationAccount == null)
            {
                return Result<TransactionDto>.Fail("La cuenta destino especificada no existe.");
            }

            if (!destinationAccount.IsActive)
            {
                return Result<TransactionDto>.Fail("La cuenta destino especificada está inactiva.");
            }

            // Actualizar balance de cuenta origen (disminuir)
            var sourceNewBalance = sourceAccount.Balance - dto.Amount;
            var sourceUpdateResult =
                await _savingAccountService.UpdateBalanceAsync(dto.SourceAccountNumber, sourceNewBalance);
            if (sourceUpdateResult.IsFailure)
            {
                return Result<TransactionDto>.Fail(sourceUpdateResult.GeneralError ??
                                                   "Error al actualizar el balance de la cuenta origen.");
            }

            // Actualizar balance de cuenta destino (aumentar)
            var destinationNewBalance = destinationAccount.Balance + dto.Amount;
            var destinationUpdateResult =
                await _savingAccountService.UpdateBalanceAsync(dto.DestinationAccountNumber, destinationNewBalance);
            if (destinationUpdateResult.IsFailure)
            {
                // Rollback: revertir el cambio en la cuenta origen
                await _savingAccountService.UpdateBalanceAsync(dto.SourceAccountNumber, sourceAccount.Balance);
                return Result<TransactionDto>.Fail(destinationUpdateResult.GeneralError ??
                                                   "Error al actualizar el balance de la cuenta destino.");
            }

            // Crear la transacción
            var transactionDebit = new Transaction
            {
                Amount = dto.Amount,
                Type = TransactionType.Debit,
                Origin = dto.SourceAccountNumber, // Cuenta origen de la transferencia
                Beneficiary = dto.DestinationAccountNumber, // Cuenta destino de la transferencia
                Date = DateTime.Now,
                Status = TransactionStatus.Approved,
                AccountNumber = dto.SourceAccountNumber, // La cuenta afectada (origen, de donde sale el dinero)
                CreatedById = dto.TellerId, // Quien hizo la transacción (el cajero)
                SubType = TransactionSubType.ThirdPartyTransfer // Tipo de transacción: Transferencia a terceros
            };
            
            var transactionCredit = new Transaction
            {
                Amount = dto.Amount,
                Type = TransactionType.Credit,
                Origin = dto.SourceAccountNumber, // Cuenta origen de la transferencia
                Beneficiary = dto.DestinationAccountNumber, // Cuenta destino de la transferencia
                Date = DateTime.Now,
                Status = TransactionStatus.Approved,
                AccountNumber = dto.DestinationAccountNumber, // La cuenta afectada (origen, de donde sale el dinero)
                CreatedById = dto.TellerId, // Quien hizo la transacción (el cajero)
                SubType = TransactionSubType.ThirdPartyTransfer // Tipo de transacción: Transferencia a terceros
            };

            var savedTransactionDebit = await _transactionRepository.AddAsync(transactionDebit);
            var savedTransactionCredit = await _transactionRepository.AddAsync(transactionCredit);
            
            
            var clientSource = await _accountServiceForWebApp.GetUserById(sourceAccount.ClientId);
            if (clientSource == null)
            {
                return Result<TransactionDto>.Fail("No se encontró al cliente");
            }
            
            var clientDestiny = await _accountServiceForWebApp.GetUserById(destinationAccount.ClientId);
            if (clientDestiny == null)
            {
                return Result<TransactionDto>.Fail("No se encontró al cliente");
            }
            
            await _emailService.SendTemplateEmailAsync(new EmailTemplateDataDto()
            {
                Type = EmailType.ExpressTransferSender,
                To = clientSource.Email,
                Variables =
                {
                    ["DestAccountLast4"] = destinationAccount.Id[^4..],
                    ["Amount"] = dto.Amount.ToString("N2"),
                    ["Date"] = DateTime.Now.ToShortDateString(),
                    ["Time"] = DateTime.Now.ToShortTimeString()
               }
            });
            
            await _emailService.SendTemplateEmailAsync(new EmailTemplateDataDto()
            {
                // clientDestiny.Email,
                Type = EmailType.ExpressTransferReceiver,
                To = "urenaperdomo@gmail.com",
                Variables =
                {
                    ["OriginAccountLast4"] = sourceAccount.Id[^4..],
                    ["Amount"] = dto.Amount.ToString("N2"),
                    ["Date"] = DateTime.Now.ToShortDateString(),
                    ["Time"] = DateTime.Now.ToShortTimeString()
                }
            });


            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result<TransactionDto>.Fail($"Error al procesar la transferencia: {ex.Message}");
        }
    }

    public async Task<Result<TransactionDto>> ProcessTellerCreditCardPaymentAsync(TellerCreditCardPaymentDto dto)
    {
        try
        {
            // Validar cuenta origen
            var sourceAccount = await _savingAccountRepository.GetByAccountNumberAsync(dto.SourceAccountNumber);
            if (sourceAccount == null)
            {
                return Result<TransactionDto>.Fail("La cuenta origen especificada no existe.");
            }

            if (!sourceAccount.IsActive)
            {
                return Result<TransactionDto>.Fail("La cuenta origen especificada está inactiva.");
            }

            // Validar que hay suficiente saldo
            if (sourceAccount.Balance < dto.Amount)
            {
                return Result<TransactionDto>.Fail(
                    "La cuenta origen no tiene suficiente saldo para realizar este pago.");
            }

            // Validar tarjeta de crédito
            var creditCard = await _creditCardRepository.GetByCardNumberAsync(dto.CreditCardNumber);
            if (creditCard == null)
            {
                return Result<TransactionDto>.Fail("La tarjeta de crédito especificada no existe.");
            }

            if (!creditCard.IsActive)
            {
                return Result<TransactionDto>.Fail("La tarjeta de crédito especificada está inactiva.");
            }
            
            var amountToPay = Math.Min(dto.Amount, creditCard.Balance);

            // Actualizar balance de cuenta origen (disminuir)
            var sourceNewBalance = sourceAccount.Balance - amountToPay;
            var sourceUpdateResult =
                await _savingAccountService.UpdateBalanceAsync(dto.SourceAccountNumber, sourceNewBalance);
            if (sourceUpdateResult.IsFailure)
            {
                return Result<TransactionDto>.Fail(sourceUpdateResult.GeneralError ??
                                                   "Error al actualizar el balance de la cuenta origen.");
            }

            // Actualizar balance de la tarjeta de crédito (disminuir deuda)
            var creditCardNewBalance = creditCard.Balance - amountToPay;
            if (creditCardNewBalance < 0)
            {
                creditCardNewBalance = 0; // No puede ser negativo
            }
            
            await _creditCardService.UpdateBalanceAsync(creditCard.CardNumber, creditCardNewBalance);

            // Crear la transacción
            var transaction = new Transaction
            {
                Amount = amountToPay,
                Type = TransactionType.Debit,
                Origin = dto.SourceAccountNumber, // Cuenta de donde se paga
                Beneficiary = dto.CreditCardNumber, // Tarjeta de crédito que se paga
                Date = DateTime.Now,
                Status = TransactionStatus.Approved,
                AccountNumber = dto.SourceAccountNumber, // La cuenta afectada (de donde sale el dinero)
                CreatedById = dto.TellerId, // Quien hizo la transacción (el cajero)
                SubType = TransactionSubType.CreditCardPayment // Tipo de transacción: Pago de tarjeta de crédito
            };
            
            
            var client = await _accountServiceForWebApp.GetUserById(sourceAccount.ClientId);
            if (client == null)
            {
                return Result<TransactionDto>.Fail("No se encontró al cliente");
            }
            
            await _emailService.SendTemplateEmailAsync(new EmailTemplateDataDto()
            {
                Type = EmailType.CreditCardPayment,
                To = client.Email,
                Variables =
                {
                    ["CardLast4"] = creditCard.CardNumber[^4..],
                    ["AccountLast4"] = sourceAccount.Id[^4..],
                    ["Amount"] = amountToPay.ToString("N2"),
                    ["Date"] = DateTime.Now.ToShortDateString(),
                    ["Time"] = DateTime.Now.ToShortTimeString()
                }
            });
            

            var savedTransaction = await _transactionRepository.AddAsync(transaction);
            var transactionDto = _mapper.Map<TransactionDto>(savedTransaction);

            return Result<TransactionDto>.Ok(transactionDto);
        }
        catch (Exception ex)
        {
            return Result<TransactionDto>.Fail($"Error al procesar el pago de tarjeta de crédito: {ex.Message}");
        }
    }

    public async Task<Result<TransactionDto>> ProcessTellerLoanPaymentAsync(TellerLoanPaymentDto dto)
    {
        try
        {
            // Validar cuenta origen
            var sourceAccount = await _savingAccountRepository.GetByAccountNumberAsync(dto.SourceAccountNumber);
            if (sourceAccount == null)
            {
                return Result<TransactionDto>.Fail("La cuenta origen especificada no existe.");
            }

            if (!sourceAccount.IsActive)
            {
                return Result<TransactionDto>.Fail("La cuenta origen especificada está inactiva.");
            }

            // Validar que hay suficiente saldo
            if (sourceAccount.Balance < dto.Amount)
            {
                return Result<TransactionDto>.Fail(
                    "La cuenta origen no tiene suficiente saldo para realizar este pago.");
            }

            // Validar préstamo
            var loan = await _loanRepository.GetByLoanNumberAsync(dto.LoanNumber);
            if (loan == null)
            {
                return Result<TransactionDto>.Fail("El préstamo especificado no existe.");
            }

            if (loan.Completed)
            {
                return Result<TransactionDto>.Fail("El préstamo especificado ya está completado.");
            }

            // Resolver ILoanService de forma diferida para evitar dependencia circular
            var loanService = _serviceProvider.GetRequiredService<ILoanService>();
            var moneyUsedToPayResult = await loanService.PayAsync(loan.Id, dto.Amount);

            if (moneyUsedToPayResult.IsFailure)
            {
                Result<TransactionDto>.Fail(moneyUsedToPayResult.GeneralError!);
            }

            var moneyUsedToPay = moneyUsedToPayResult.Value!;

            // Actualizar balance de cuenta origen (disminuir)
            var sourceNewBalance = sourceAccount.Balance - moneyUsedToPay;
            var sourceUpdateResult =
                await _savingAccountService.UpdateBalanceAsync(dto.SourceAccountNumber, sourceNewBalance);
            if (sourceUpdateResult.IsFailure)
            {
                return Result<TransactionDto>.Fail(sourceUpdateResult.GeneralError ??
                                                   "Error al actualizar el balance de la cuenta origen.");
            }

            // Aquí deberías actualizar el préstamo (disminuir el monto pendiente)
            // Por ahora, solo registramos la transaccion - ALNA

            // Crear la transacción
            var transaction = new Transaction
            {
                Amount = moneyUsedToPay,
                Type = TransactionType.Debit,
                Origin = dto.SourceAccountNumber, // Cuenta de donde se paga
                Beneficiary = dto.LoanNumber, // Préstamo que se paga
                Date = DateTime.Now,
                Status = TransactionStatus.Approved,
                AccountNumber = dto.SourceAccountNumber, // La cuenta afectada (de donde sale el dinero)
                CreatedById = dto.TellerId, // Quien hizo la transacción (el cajero)
                SubType = TransactionSubType.LoanPayment // Tipo de transacción: Pago de préstamo
            };

            
            var savedTransaction = await _transactionRepository.AddAsync(transaction);
            var transactionDto = _mapper.Map<TransactionDto>(savedTransaction);
                
                
            var client = await _accountServiceForWebApp.GetUserById(sourceAccount.ClientId);
            if (client == null)
            {
                return Result<TransactionDto>.Fail("No se encontró al cliente");
            }
            
            await _emailService.SendTemplateEmailAsync(new EmailTemplateDataDto()
            {
                Type = EmailType.LoanPayment,
                To = client.Email,
                Variables =
                {
                    ["LoanNumber"] = loan.Id[^4..],
                    ["AccountLast4"] = sourceAccount.Id[^4..],
                    ["Amount"] = moneyUsedToPay.ToString("N2"),
                    ["Date"] = DateTime.Now.ToShortDateString(),
                    ["Time"] = DateTime.Now.ToShortTimeString()
                }
            });

            return Result<TransactionDto>.Ok(transactionDto);
        }
        catch (Exception ex)
        {
            return Result<TransactionDto>.Fail($"Error al procesar el pago de préstamo: {ex.Message}");
        }
    }

    
    /*
     * Este metodo entrega toda la info de las transacctiones de manera general por eso ven el (Todas)
     * Basicamente te va a dar todo de manera bruta sin filtros.
     * 
     */
    public async Task<Result<TransactionSummaryDto>> GetTransactionSummaryAsync()
    {
        try
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            // Obtener todas las transacciones (sin filtrar por cajero)
            var allTransactions = _transactionRepository.GetAllQueryable();
            
            // Total de transacciones (todas)
            var totalTransactions = await allTransactions.CountAsync();
            
            // Transacciones de hoy (todas)
            var todayTransactions = await allTransactions
                .Where(t => t.Date >= today && t.Date < tomorrow)
                .CountAsync();
            
            // Total de depósitos (todas)
            var totalDeposits = await allTransactions
                .Where(t => t.SubType == TransactionSubType.Deposit)
                .CountAsync();
            
            // Depósitos de hoy (todas)
            var todayDeposits = await allTransactions
                .Where(t => t.SubType == TransactionSubType.Deposit 
                         && t.Date >= today 
                         && t.Date < tomorrow)
                .CountAsync();
            
            // Total de retiros (todas)
            var totalWithdrawals = await allTransactions
                .Where(t => t.SubType == TransactionSubType.Withdrawal)
                .CountAsync();
            
            // Retiros de hoy (todas)
            var todayWithdrawals = await allTransactions
                .Where(t => t.SubType == TransactionSubType.Withdrawal 
                         && t.Date >= today 
                         && t.Date < tomorrow)
                .CountAsync();
            
            // Total de pagos (pagos de tarjeta + préstamos)
            var totalPayments = await allTransactions
                .Where(t => t.SubType == TransactionSubType.CreditCardPayment 
                         || t.SubType == TransactionSubType.LoanPayment)
                .CountAsync();
            
            // Pagos de hoy
            var todayPayments = await allTransactions
                .Where(t => (t.SubType == TransactionSubType.CreditCardPayment 
                          || t.SubType == TransactionSubType.LoanPayment)
                         && t.Date >= today 
                         && t.Date < tomorrow)
                .CountAsync();
            
            // Construir el resumen
            var summary = new TransactionSummaryDto
            {
                TotalTransactions = totalTransactions,
                TodayTransactions = todayTransactions,
                TotalDeposits = totalDeposits,
                TodayDeposits = todayDeposits,
                TotalWithdrawals = totalWithdrawals,
                TodayWithdrawals = todayWithdrawals,
                TotalPayments = totalPayments,
                TodayPayments = todayPayments
            };

            return Result<TransactionSummaryDto>.Ok(summary);
        }
        catch (Exception ex)
        {
            return Result<TransactionSummaryDto>.Fail($"Error al obtener el resumen de transacciones: {ex.Message}");
        }
    }

    public async Task<Result<TransactionSummaryDto>> GetTellerTransactionSummaryAsync(string tellerId)
    {
        try
        {
            var summary = new TransactionSummaryDto
            {
                TotalTransactions = await _transactionRepository.GetTotalTransactionsByTellerAsync(tellerId),
                TodayTransactions = await _transactionRepository.GetTodayTransactionsByTellerAsync(tellerId),
                TotalDeposits = await _transactionRepository.GetTotalDepositsByTellerAsync(tellerId),
                TodayDeposits = await _transactionRepository.GetTodayDepositsByTellerAsync(tellerId),
                TotalWithdrawals = await _transactionRepository.GetTotalWithdrawalsByTellerAsync(tellerId),
                TodayWithdrawals = await _transactionRepository.GetTodayWithdrawalsByTellerAsync(tellerId),
                TotalPayments = await _transactionRepository.GetTotalPaymentsByTellerAsync(tellerId),
                TodayPayments = await _transactionRepository.GetTodayPaymentsByTellerAsync(tellerId)
            };

            return Result<TransactionSummaryDto>.Ok(summary);
        }
        catch (Exception ex)
        {
            return Result<TransactionSummaryDto>.Fail($"Error al obtener el resumen de transacciones: {ex.Message}");
        }
    }

    public async Task<Result<PaginatedData<TransactionDto>>> GetTransactionsByAccountAsync(string accountNumber, int page, int pageSize)
    {
        try
        {
            var query = _transactionRepository.GetAllQueryable()
                .AsNoTracking()
                .Where(t => t.AccountNumber == accountNumber);

          
            var totalItems = await query.CountAsync();
            
            var transactions = await query
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);
            
            var paginatedData = new PaginatedData<TransactionDto>(
                transactionDtos,
                totalItems,
                page,
                pageSize);

            return Result<PaginatedData<TransactionDto>>.Ok(paginatedData);
        }
        catch (Exception ex)
        {
            return Result<PaginatedData<TransactionDto>>.Fail(
                $"Error al obtener las transacciones: {ex.Message}");
        }
    }
    
    public async Task<Result<TransactionDto>> ProcessLoanDisbursementTransfer(LoanDisbursementTransactionDto dto)
    {
        var loan = await _loanRepository.GetAllQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == dto.SourceLoanNumber);

        if (loan == null)
            return Result<TransactionDto>.Fail("No se encontró ningún préstamo con ese ID");

        var savingAccountResult = await _savingAccountService.GetByIdAsync(dto.DestinationAccountNumber);
        if (savingAccountResult.IsFailure)
            return Result<TransactionDto>.Fail(savingAccountResult.GeneralError!);

        if (!savingAccountResult.Value!.IsPrincipalAccount)
            return Result<TransactionDto>.Fail("esta no es una cuenta principal");

        var transactionDto = new TransactionDto
        {
            Id = 0,
            Amount = dto.Amount,
            CreatedById = dto.AprovedByAdminId,
            AccountNumber = dto.DestinationAccountNumber,
            Type = TransactionType.Credit,
            Beneficiary = dto.DestinationAccountNumber,
            Origin = dto.SourceLoanNumber,
            Date = DateTime.Now,
            SubType = TransactionSubType.Deposit,
            Status = TransactionStatus.Approved,
        };

        var transactionResult = await AddAsync(transactionDto);

        if (transactionResult.IsFailure)
            return transactionResult;

        var depositResult = await _savingAccountService.DepositToAccountAsync(dto.DestinationAccountNumber, dto.Amount);

        if (depositResult.IsFailure)
            return Result<TransactionDto>.Fail(depositResult.GeneralError!);

        // para el email
        var client = await _accountServiceForWebApp.GetUserById(loan.ClientId);
        if (client is null)
        {
            return Result<TransactionDto>.Fail("no se encontro el cliente");
        }

        var account = savingAccountResult.Value!;

        var newMontly = MontlyPayment.Calculate(loan.Amount, loan.AnualRate, loan.TermMonths);

        await _emailService.SendTemplateEmailAsync(new EmailTemplateDataDto()
        {
            Type = EmailType.LoanApproved,
            To = client.Email,
            Variables =
            {
                ["LoanNumber"] = loan.Id,
                ["Amount"] = loan.Amount.ToString("N2"),
                ["Term"] = loan.TermMonths.ToString(),
                ["InterestRate"] = loan.TermMonths.ToString(),
                ["MonthlyPayment"] = newMontly.ToString("N2"),
            }
        });

        return transactionResult;
    }

    public async Task<Result> TransferAsync(string userId, TransferDto dto)
    {
        try
        {
            var origin = await _savingAccountRepository.GetByAccountNumberAsync(dto.FromAccountNumber);
            var destination = await _savingAccountRepository.GetByAccountNumberAsync(dto.ToAccountNumber);

            if (origin == null || destination == null)
                return Result.Fail("Cuenta no encontrada");

            if (!origin.IsActive || !destination.IsActive)
                return Result.Fail("Una de las cuentas está inactiva");

            if (origin.Balance < dto.Amount)
                return Result.Fail("Fondos insuficientes");

            // Balance
            origin.Balance -= dto.Amount;
            destination.Balance += dto.Amount;

            await _savingAccountRepository.UpdateAsync(origin.Id, origin);
            await _savingAccountRepository.UpdateAsync(destination.Id, destination);

            var transactionDebit = new Transaction
            {
                Amount = dto.Amount,
                AccountNumber = origin.Id,
                CreatedById = userId,
                Type = TransactionType.Debit,
                SubType = TransactionSubType.ThirdPartyTransfer,
                Beneficiary = destination.Id,
                Origin = origin.Id,
                Date = DateTime.Now,
                Status = TransactionStatus.Approved
            };

            var transactionCredit = new Transaction
            {
                Amount = dto.Amount,
                AccountNumber = destination.Id,
                CreatedById = userId,
                Type = TransactionType.Credit,
                SubType = TransactionSubType.ThirdPartyTransfer,
                Beneficiary = destination.Id,
                Origin = origin.Id,
                Date = DateTime.Now,
                Status = TransactionStatus.Approved
            };

            await _transactionRepository.AddAsync(transactionDebit);
            await _transactionRepository.AddAsync(transactionCredit);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error en transferencia: {ex.Message}");
        }
    }




}