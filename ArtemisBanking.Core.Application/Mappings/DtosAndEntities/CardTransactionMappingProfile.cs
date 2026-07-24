using ArtemisBanking.Core.Application.Dtos.Beneficiary;
using ArtemisBanking.Core.Application.Dtos.CardTransaction;
using ArtemisBanking.Core.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndEntities;

public class CardTransactionMappingProfile : Profile 
{
    public CardTransactionMappingProfile()
    {
        CreateMap<CardTransaction, CardTransactionDto>().ReverseMap();
    }
}