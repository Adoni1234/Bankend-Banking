using ArtemisBanking.Core.Application;
using ArtemisBanking.Core.Application.Dtos.Loan;
using ArtemisBanking.Core.Application.Dtos.LoanInstallment;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Core.Domain.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingApi.Controllers;

[Authorize(Roles = $"{nameof(Roles.Admin)}")]
public class LoanController : BaseApiController
{
   private readonly ILoanService _loanService;
   private readonly IRiskService _riskService;
   private readonly IMapper _mapper;

   public LoanController(ILoanService loanService, IRiskService riskService, IMapper mapper)
   {
      _loanService = loanService;
      _riskService = riskService;
      _mapper = mapper;
   }

   [HttpGet]
   [ProducesResponseType(StatusCodes.Status200OK,  Type = typeof(PaginatedData<SimpleLoanApiDto>))]
   [ProducesResponseType(StatusCodes.Status400BadRequest)]
   [ProducesResponseType(StatusCodes.Status401Unauthorized)]
   [ProducesResponseType(StatusCodes.Status500InternalServerError)]
   public async Task<IActionResult> Get([FromQuery] LoanFiltersDto filters)
   {

      try
      {
         if (!ModelState.IsValid)
         {
            return BadRequest(ModelState);
         }
         
         var loansPaged = await _loanService.GetLoansPagedAsync(
            filters.Page,
            filters.PageSize,
            filters.IdentityCardNumber,
            filters.IsCompleted);
         var simpleLoans = loansPaged.Items.Select(loan => new SimpleLoanApiDto
         {
            Id = loan.Id,
            Client = $"{loan.Client!.FirstName} {loan.Client.LastName}",
            Amount = loan.Amount,
            TermMonths = loan.TermMonths,
            AnualRate = loan.AnualRate,
            Completed = loan.Completed,
            IsDue = loan.IsDue,
            InstallmentsCount = loan.InstallmentsCount,
            InstallmentsPaidCount = loan.InstallmentsPaidCount,
            RemainingBalanceToPay = loan.RemainingBalanceToPay,
         });
         return Ok(simpleLoans);
      }
      catch (Exception e)
      {
         return BadRequest(e.Message);
      }
   }

   [HttpPost]
   [ProducesResponseType(StatusCodes.Status201Created)]
   [ProducesResponseType(StatusCodes.Status400BadRequest)]
   [ProducesResponseType(StatusCodes.Status409Conflict)]
   [ProducesResponseType(StatusCodes.Status401Unauthorized)]
   [ProducesResponseType(StatusCodes.Status500InternalServerError)]
   public async Task<IActionResult> Post([FromBody] CreateLoanApiDto dto)
   {

      try
      {
         if (!ModelState.IsValid)
         {
            return BadRequest(ModelState);
         }

         var averageClientDebtOfSystem = await _riskService.GetSystemAverageClientDebtAsync();
         var totalDebtOfUser = await _riskService.CalculateClientTotalDebt(dto.ClientId);
         var capitalWithInterests = _riskService.CalculateTotalLoanDebt(dto.Amount, dto.AnualRate, dto.TermMonths);

         // if (totalDebtOfUser > averageClientDebtOfSystem || (totalDebtOfUser + capitalWithInterests) > averageClientDebtOfSystem)
         // {
         //    return Conflict("El clientee se convierte de alto riesgo");
         // }

         var createResult = await _loanService.AddAsync(new LoanDto
         {
            ClientId = dto.ClientId,
            ApprovedByUserId = User.FindFirst("uid")?.Value ?? "",
            Amount = dto.Amount,
            TermMonths = dto.TermMonths,
            AnualRate = dto.AnualRate,
            Completed = false,
            IsDue = false,
            CreatedAt = DateTime.Now
         });

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

   [HttpGet("{id}")]
   [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoanApiWithDetails))]
   [ProducesResponseType(StatusCodes.Status404NotFound)]
   [ProducesResponseType(StatusCodes.Status401Unauthorized)]
   [ProducesResponseType(StatusCodes.Status500InternalServerError)]
   public async Task<IActionResult> GetLoanWithDetails([FromRoute] string id)
   {
      try
      {
         if (!ModelState.IsValid)
         {
            return BadRequest(ModelState);
         }

         var loan = await _loanService.GetAmortizationTableAsync(id);
         if (loan.IsFailure)
         {
            return BadRequest400WithErrorMessagesFromResult(loan);
         }

         var loanDetails = new LoanApiWithDetails
         {
            Id = loan.Value!.Id,
            ClientId = loan.Value.ClientId,
            Amount = loan.Value.Amount,
            TermMonths = loan.Value.TermMonths,
            AnualRate = loan.Value.AnualRate,
            Completed = loan.Value.Completed,
            IsDue = loan.Value.IsDue, 
            LoanInstallments = _mapper.Map<List<SimpleLoanInstallmentApiDto>>(loan.Value.LoanInstallments),
         };
         return Ok(loanDetails);
      }
      catch (Exception e)
      {
         return BadRequest(e.Message);
      }
   }  
   
   [HttpPatch("{id}/rate")]
   [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoanApiWithDetails))]
   [ProducesResponseType(StatusCodes.Status404NotFound)]
   [ProducesResponseType(StatusCodes.Status401Unauthorized)]
   [ProducesResponseType(StatusCodes.Status500InternalServerError)]
   public async Task<IActionResult> updateLimit([FromRoute] string id, UpdateLoanApiRateDto dto)
   {

      try
      {
         if (!ModelState.IsValid)
         {
            return BadRequest(ModelState);
         }

         var loanExists = await _loanService.GetByIdAsync(id);
         if (loanExists.IsFailure)
         {
            return NotFound();
         }
         
         var loan = await _loanService.UpdateInterestRateAsync(id, dto.NewAnualRate);
         if (loan.IsFailure)
         {
            return BadRequest400WithErrorMessagesFromResult(loan);
         }

         return NoContent();
      }
      catch (Exception e)
      {
         return BadRequest(e.Message);
      }
   }  
   
}