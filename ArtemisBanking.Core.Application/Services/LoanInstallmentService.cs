using ArtemisBanking.Core.Application.Dtos.LoanInstallment;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Core.Application.Services;

public class LoanInstallmentService : GenericServices<int, LoanInstallment, LoanInstallmentDto>, ILoanInstallmentService
{
    private readonly ILoanInstallmentRepository _loanInstallmentRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IMapper _mapper;

    public LoanInstallmentService(ILoanInstallmentRepository repository, IMapper mapper, 
        ILoanRepository loanRepository) : base(repository, mapper)
    {
        _loanInstallmentRepository = repository;
        _mapper = mapper;
        _loanRepository = loanRepository;
    }

    public async Task<Result<List<LoanInstallmentDto>>> GetInstallmentsOfLoan(string loanId)
    {
        var loan = await _loanInstallmentRepository.GetAllQueryable().AsNoTracking()
            .Where(x => x.LoanId == loanId)
            .ToListAsync();

        if (loan.Count == 0)
        {
            return Result<List<LoanInstallmentDto>>.Fail("No se encontraron cuotas para ese préstamo");
        }

        return Result<List<LoanInstallmentDto>>.Ok(_mapper.Map<List<LoanInstallmentDto>>(loan));
    }
}