using ArtemisBanking.Core.Application.Dtos.Beneficiary;
using ArtemisBanking.Core.Application.Dtos.SavingAccount;
using ArtemisBanking.Core.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndEntities;

public class SavingAccountMappingProfile : Profile 
{
    public SavingAccountMappingProfile()
    {
        CreateMap<SavingAccount, SavingAccountDto>().ReverseMap();
    }
}