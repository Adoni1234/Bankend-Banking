using ArtemisBanking.Core.Application.Dtos.Commerce;
using ArtemisBanking.Core.Application.ViewModels.Commerce;
using ArtemisBanking.Core.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndViewModels;

public class CommerceDtoMappingProfile : Profile 
{
    public CommerceDtoMappingProfile()
    {
        CreateMap<CommerceDto, CommerceViewModel>().ReverseMap();
    }
}