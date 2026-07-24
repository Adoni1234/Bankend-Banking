using ArtemisBanking.Core.Application.Dtos.Beneficiary;
using ArtemisBanking.Core.Application.Dtos.Transaction;
using ArtemisBanking.Core.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndEntities;

public class TransactionMappingProfile : Profile 
{
    public TransactionMappingProfile()
    {
        CreateMap<Transaction, TransactionDto>().ReverseMap();
    }
}