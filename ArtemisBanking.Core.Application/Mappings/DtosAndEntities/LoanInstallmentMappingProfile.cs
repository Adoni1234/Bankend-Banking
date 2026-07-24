using ArtemisBanking.Core.Application.Dtos.Beneficiary;
using ArtemisBanking.Core.Application.Dtos.LoanInstallment;
using ArtemisBanking.Core.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndEntities;

public class LoanInstallmentMappingProfile : Profile 
{
    public LoanInstallmentMappingProfile()
    {
        CreateMap<LoanInstallment, LoanInstallmentDto>().ReverseMap();
    }
}