using ArtemisBanking.Core.Application;
using ArtemisBanking.Core.Application.Dtos.CardTransaction;
using ArtemisBanking.Core.Application.Dtos.CreditCard;
using ArtemisBanking.Core.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingApi.Controllers;

public class CreditCardController : BaseApiController
{
    private readonly ICreditCardService _creditCardService;
    private readonly ICardTransactionService _cardTransactionService;

    public CreditCardController(ICreditCardService creditCardService, ICardTransactionService cardTransactionService)
    {
        _creditCardService = creditCardService;
        _cardTransactionService = cardTransactionService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedData<SimpleCreditCardApiDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get([FromQuery]CreditCardFiltersDto filters)
    {

        try
        {
            var creditCards = await _creditCardService.GetCreditCardsPagedAsync(
                filters.Page,
                filters.PageSize,
                filters.IdentityCardNumber,
                filters.IsActive
            );

            var simpleCreditCardsDtos = creditCards.Items.Select(c => new SimpleCreditCardApiDto
            {
                CardNumber = c.CardNumber,
                Client = $"{c.Client?.FirstName} {c.Client?.LastName}",
                ExpirationDate = $"{c.ExpirationMonth}/{c.ExpirationYear}",
                Limit = c.CreditLimit,
                Balance = c.Balance,
                Status = c.IsActive ? "Active" : "Canceled"
            });

            var simpleCreditCardsPaged =
                new PaginatedData<SimpleCreditCardApiDto>(simpleCreditCardsDtos, creditCards.Pagination);
            return Ok(simpleCreditCardsPaged);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCreditCard(CreateCreditCardApiDto dto)
    {

        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var createResult = await _creditCardService.CreateNewCreditCard(
                User.FindFirst("uid")?.Value ?? "",
                dto.ClientId,
                dto.Limit);
            if (createResult.IsFailure)
            {
                return BadRequest400WithErrorMessagesFromResult(createResult);
            }

            return Created();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

    }
    
    [HttpGet("{cardNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK,  Type = typeof(List<CardTransactionDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Details([FromRoute] string cardNumber)
    {

        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var exists = await _creditCardService.GetByIdAsync(cardNumber);
            if (exists.IsFailure)
            {
                return NotFound();
            }
            
            var creditcard = await _cardTransactionService.GetByCreditCardIdAsync(cardNumber);
            return Ok(creditcard.Value!.CardTransactions);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

    }
    
    [HttpPatch("{cardNumber}/limit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLimit([FromRoute] string cardNumber, [FromBody] UpdateCreditcardLimitApiDto dto)
    {

        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var exists = await _creditCardService.GetByIdAsync(cardNumber);
            if (exists.IsFailure)
            {
                return NotFound();
            }
            
            var updateResult = await _creditCardService.UpdateLimitAsync(cardNumber, dto.NewLimit);
            if (updateResult.IsFailure)
            {
                return BadRequest400WithErrorMessagesFromResult(updateResult);
            }

            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

    }
    
    [HttpPatch("{cardNumber}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Cancel([FromRoute] string cardNumber)
    {

        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var exists = await _creditCardService.GetByIdAsync(cardNumber);
            if (exists.IsFailure)
            {
                return NotFound();
            }

            var updateResult = await _creditCardService.CancelCardAsync(cardNumber);
            if (updateResult.IsFailure)
            {
                return BadRequest400WithErrorMessagesFromResult(updateResult);
            }

            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

    }
}
