using ArtemisBanking.Core.Application.Dtos.Beneficiary;
using ArtemisBanking.Core.Application.ViewModels.Beneficiary;
using ArtemisBanking.Core.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndViewModels;

public class BeneficiaryDtoMappingProfile : Profile 
{
    public BeneficiaryDtoMappingProfile()
    {
        CreateMap<BeneficiaryDto, BeneficiaryViewModel>().ReverseMap();
    }
}