using ArtemisBanking.Core.Application.Dtos.CardTransaction;
using ArtemisBanking.Core.Application.ViewModels.CardTransaction;
using ArtemisBanking.Core.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndViewModels;

public class CardTransactionDtoMappingProfile : Profile 
{
    public CardTransactionDtoMappingProfile()
    {
        CreateMap<CardTransactionDto, CardTransactionViewmodel>().ReverseMap();
    }
}