using ArtemisBanking.Core.Application.Dtos.SavingAccount;
using ArtemisBanking.Core.Application.ViewModels.SavingAccount;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndViewModels;

public class SavingAccountDtoMappingProfile : Profile 
{
    public SavingAccountDtoMappingProfile()
    {
        CreateMap<SavingAccountDto, SavingAccountViewModel>().ReverseMap();
    }
}