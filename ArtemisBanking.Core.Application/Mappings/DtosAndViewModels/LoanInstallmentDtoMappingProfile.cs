using ArtemisBanking.Core.Application.Dtos.LoanInstallment;
using ArtemisBanking.Core.Application.ViewModels.LoanInstallment;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndViewModels;

public class LoanInstallmentDtoMappingProfile : Profile 
{
    public LoanInstallmentDtoMappingProfile()
    {
        CreateMap<LoanInstallmentDto, LoanInstallmentViewModel>().ReverseMap();
        CreateMap<LoanInstallmentDto, SimpleLoanInstallmentApiDto>().ReverseMap();
    }
}