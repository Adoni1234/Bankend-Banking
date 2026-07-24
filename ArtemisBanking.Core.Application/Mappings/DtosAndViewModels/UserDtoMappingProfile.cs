using ArtemisBanking.Core.Application.Dtos.Loan;
using ArtemisBanking.Core.Application.Dtos.User;
using ArtemisBanking.Core.Application.ViewModels.Loan;
using ArtemisBanking.Core.Application.ViewModels.User;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndViewModels;

public class UserDtoMappingProfile : Profile
{
    public UserDtoMappingProfile()
    {
        CreateMap<UserDto, UserViewModel>().ReverseMap();
        CreateMap<ClientsWithDebtDto, ClientsWithDebtViewModel>().ReverseMap();
        CreateMap<CreateApiUserDto, UserSaveDto>().ReverseMap();
        CreateMap<UpdateApiUserDto, UserSaveDto >().ReverseMap();
        CreateMap<UserDto, UserApiDto>().ReverseMap();
    }
}