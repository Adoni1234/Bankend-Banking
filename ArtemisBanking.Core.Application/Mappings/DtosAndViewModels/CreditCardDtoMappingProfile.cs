using ArtemisBanking.Core.Application.Dtos.CreditCard;
using ArtemisBanking.Core.Application.ViewModels.CreditCard;
using ArtemisBanking.Core.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndViewModels;

public class CreditCardDtoMappingProfile : Profile 
{
    public CreditCardDtoMappingProfile()
    {
        CreateMap<CreditCardDto, CreditCardViewModel>().ReverseMap();
    }
}