using ArtemisBanking.Core.Application.Dtos.Loan;
using ArtemisBanking.Core.Application.Dtos.LoanInstallment;
using ArtemisBanking.Core.Application.ViewModels.Loan;
using ArtemisBanking.Core.Application.ViewModels.LoanInstallment;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndViewModels;

public class LoanDtoMappingProfile : Profile 
{
    public LoanDtoMappingProfile()
    {
        CreateMap<LoanDto, LoanViewModel>().ReverseMap();
        CreateMap<LoanSummaryDto, LoanSummaryViewModel>().ReverseMap();
    }
}