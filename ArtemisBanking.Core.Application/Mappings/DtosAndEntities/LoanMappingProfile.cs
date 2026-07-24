using ArtemisBanking.Core.Application.Dtos.Beneficiary;
using ArtemisBanking.Core.Application.Dtos.Loan;
using ArtemisBanking.Core.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndEntities;

public class LoanMappingProfile : Profile 
{
    public LoanMappingProfile()
    {
        CreateMap<Loan, LoanDto>().ReverseMap();
    }
}