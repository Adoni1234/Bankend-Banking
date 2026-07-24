using ArtemisBanking.Core.Application.Dtos.Beneficiary;
using ArtemisBanking.Core.Application.Dtos.Commerce;
using ArtemisBanking.Core.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndEntities;

public class CommerceMappingProfile : Profile 
{
    public CommerceMappingProfile()
    {
        CreateMap<Commerce, CommerceDto>().ReverseMap();
    }
}