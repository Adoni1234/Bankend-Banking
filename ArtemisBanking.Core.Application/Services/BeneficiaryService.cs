using ArtemisBanking.Core.Application.Dtos.Beneficiary;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Core.Application.Services;

public class BeneficiaryService : GenericServices<int, Beneficiary, BeneficiaryDto>, IBeneficiaryService 
{
    private readonly IBeneficiaryRepository _beneficiaryRepository;
    protected readonly IMapper _mapper;

    public BeneficiaryService(IBeneficiaryRepository repository, IMapper mapper) : base(repository, mapper)
    {
        _beneficiaryRepository = repository;
        _mapper = mapper;
    }

    public async Task<List<BeneficiaryDto>> GetByUserIdAsync(string userId)
    {
        var list = await _beneficiaryRepository
            .GetAllQueryable()
            .Where(x => x.UserId == userId)
            .ToListAsync();

        return _mapper.Map<List<BeneficiaryDto>>(list);
    }

    public async Task AddAsync(string userId, string savingAccountId)
    {
        var entity = new Beneficiary
        {
            UserId = userId,
            SavingAccountId = savingAccountId
        };

        await _beneficiaryRepository.AddAsync(entity);
    }



}