using ArtemisBanking.Core.Application;
using ArtemisBanking.Core.Application.Dtos.Client;
using ArtemisBanking.Core.Application.Dtos.SavingAccount;
using ArtemisBanking.Core.Application.Dtos.Transaction;
using ArtemisBanking.Core.Application.Dtos.Transaction.Teller;
using ArtemisBanking.Core.Application.Dtos.Transfer;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingApi.Controllers;

// Rutas resultantes: api/v1/client/home, api/v1/client/accounts, etc.
[Authorize(Roles = nameof(Roles.Client))]
public class ClientController : BaseApiController
{
    private readonly ISavingAccountService _savingAccountService;
    private readonly ITransactionService _transactionService;
    private readonly IBeneficiaryService _beneficiaryService;
    private readonly ICreditCardService _creditCardService;
    private readonly ILoanService _loanService;

    public ClientController(
        ISavingAccountService savingAccountService,
        ITransactionService transactionService,
        IBeneficiaryService beneficiaryService,
        ICreditCardService creditCardService,
        ILoanService loanService)
    {
        _savingAccountService = savingAccountService;
        _transactionService = transactionService;
        _beneficiaryService = beneficiaryService;
        _creditCardService = creditCardService;
        _loanService = loanService;
    }

    private string CurrentUserId => User.FindFirst("uid")?.Value ?? string.Empty;

    // GET api/v1/client/home
    [HttpGet("home")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientHomeApiDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Home()
    {
        try
        {
            var mainAccountResult = await _savingAccountService.GetMainAccountByUserIdAsync(CurrentUserId);
            if (mainAccountResult.IsFailure)
            {
                return BadRequest400WithErrorMessagesFromResult(mainAccountResult);
            }

            var mainAccount = mainAccountResult.Value!;
            var transactionsResult = await _transactionService.GetTransactionsByAccountAsync(mainAccount.Id, 1, 5);

            var recentTransactions = transactionsResult.IsSuccess
                ? transactionsResult.Value!.Items.Select(MapTransaction).ToList()
                : new List<ClientTransactionApiDto>();

            var dto = new ClientHomeApiDto
            {
                MainAccount = MapAccount(mainAccount),
                RecentTransactions = recentTransactions
            };

            return Ok(dto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    // GET api/v1/client/accounts
    [HttpGet("accounts")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ClientAccountApiDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Accounts()
    {
        try
        {
            var accounts = await _savingAccountService.GetByUserIdAsync(CurrentUserId);
            return Ok(accounts.Select(MapAccount).ToList());
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    // GET api/v1/client/transactions?accountNumber=...&page=1&pageSize=10
    [HttpGet("transactions")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedData<ClientTransactionApiDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Transactions([FromQuery] string? accountNumber, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrEmpty(accountNumber))
            {
                var mainAccountResult = await _savingAccountService.GetMainAccountByUserIdAsync(CurrentUserId);
                if (mainAccountResult.IsFailure)
                {
                    return BadRequest400WithErrorMessagesFromResult(mainAccountResult);
                }
                accountNumber = mainAccountResult.Value!.Id;
            }

            var result = await _transactionService.GetTransactionsByAccountAsync(accountNumber, page, pageSize);
            if (result.IsFailure)
            {
                return BadRequest400WithErrorMessagesFromResult(result);
            }

            var paged = new PaginatedData<ClientTransactionApiDto>(
                result.Value!.Items.Select(MapTransaction),
                result.Value.Pagination);

            return Ok(paged);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    // GET api/v1/client/beneficiaries
    [HttpGet("beneficiaries")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ClientBeneficiaryApiDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Beneficiaries()
    {
        try
        {
            var beneficiaries = await _beneficiaryService.GetByUserIdAsync(CurrentUserId);
            var dtos = beneficiaries.Select(b => new ClientBeneficiaryApiDto
            {
                Id = b.Id,
                AccountNumber = b.SavingAccountId,
                OwnerName = b.SavingAccount?.Client is { } client ? $"{client.FirstName} {client.LastName}" : null
            }).ToList();

            return Ok(dtos);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    // POST api/v1/client/beneficiaries
    [HttpPost("beneficiaries")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateBeneficiary([FromBody] CreateBeneficiaryApiDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _beneficiaryService.AddAsync(CurrentUserId, dto.AccountNumber);
            return Created();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    // DELETE api/v1/client/beneficiaries/{id}
    [HttpDelete("beneficiaries/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteBeneficiary([FromRoute] int id)
    {
        try
        {
            var result = await _beneficiaryService.DeleteAsync(id);
            if (result.IsFailure)
            {
                return BadRequest400WithErrorMessagesFromResult(result);
            }

            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    // POST api/v1/client/transfers
    [HttpPost("transfers")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Transfer([FromBody] TransferDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _transactionService.TransferAsync(CurrentUserId, dto);
            if (result.IsFailure)
            {
                return BadRequest400WithErrorMessagesFromResult(result);
            }

            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    // GET api/v1/client/payment-options
    [HttpGet("payment-options")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientPaymentOptionsApiDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PaymentOptions()
    {
        try
        {
            var accounts = await _savingAccountService.GetByUserIdAsync(CurrentUserId);
            var cards = await _creditCardService.GetByUserIdAsync(CurrentUserId);
            var loans = await _loanService.GetByUserIdAsync(CurrentUserId);

            var dto = new ClientPaymentOptionsApiDto
            {
                Accounts = accounts.Select(MapAccount).ToList(),
                CreditCards = cards.Select(c => new ClientCreditCardApiDto
                {
                    CardNumber = c.CardNumber!,
                    CreditLimit = c.CreditLimit,
                    Balance = c.Balance,
                    IsActive = c.IsActive
                }).ToList(),
                Loans = loans.Select(l => new ClientLoanApiDto
                {
                    Id = l.Id,
                    Amount = l.Amount,
                    TermMonths = l.TermMonths,
                    AnualRate = l.AnualRate,
                    Completed = l.Completed,
                    IsDue = l.IsDue
                }).ToList()
            };

            return Ok(dto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    // POST api/v1/client/payments/credit-card
    [HttpPost("payments/credit-card")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PayCreditCard([FromBody] PayCreditCardApiDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _transactionService.ProcessTellerCreditCardPaymentAsync(new TellerCreditCardPaymentDto
            {
                SourceAccountNumber = dto.SourceAccountNumber,
                CreditCardNumber = dto.CreditCardNumber,
                Amount = dto.Amount,
                TellerId = CurrentUserId
            });

            if (result.IsFailure)
            {
                return BadRequest400WithErrorMessagesFromResult(result);
            }

            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    // POST api/v1/client/payments/loan
    [HttpPost("payments/loan")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PayLoan([FromBody] PayLoanApiDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _transactionService.ProcessTellerLoanPaymentAsync(new TellerLoanPaymentDto
            {
                SourceAccountNumber = dto.SourceAccountNumber,
                LoanNumber = dto.LoanNumber,
                Amount = dto.Amount,
                TellerId = CurrentUserId
            });

            if (result.IsFailure)
            {
                return BadRequest400WithErrorMessagesFromResult(result);
            }

            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    private static ClientAccountApiDto MapAccount(SavingAccountDto a) => new()
    {
        AccountNumber = a.Id,
        Balance = a.Balance,
        IsPrincipalAccount = a.IsPrincipalAccount,
        IsActive = a.IsActive,
        CreatedAt = a.CreatedAt
    };

    private static ClientTransactionApiDto MapTransaction(TransactionDto t) => new()
    {
        Id = t.Id,
        Amount = t.Amount,
        AccountNumber = t.AccountNumber,
        Type = t.Type,
        SubType = t.SubType,
        Status = t.Status,
        Beneficiary = t.Beneficiary,
        Origin = t.Origin,
        Date = t.Date
    };
}