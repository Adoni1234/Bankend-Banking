using ArtemisBanking.Core.Application.Dtos.Beneficiary;
using ArtemisBanking.Core.Application.Dtos.Transaction;
using ArtemisBanking.Core.Application.ViewModels.Transaction;
using ArtemisBanking.Core.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndViewModels;

public class TransactionDtoMappingProfile : Profile 
{
    public TransactionDtoMappingProfile()
    {
        CreateMap<TransactionDto, TransactionViewModel>().ReverseMap();
    }
}