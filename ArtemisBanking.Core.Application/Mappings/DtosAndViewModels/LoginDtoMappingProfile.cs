using ArtemisBanking.Core.Application.Dtos.Login;
using ArtemisBanking.Core.Application.Dtos.User;
using ArtemisBanking.Core.Application.ViewModels.Login;
using AutoMapper;

namespace ArtemisBanking.Core.Application.Mappings.DtosAndViewModels;

public class LoginDtoMappingProfile :  Profile
{
    public LoginDtoMappingProfile()
    {
        
        CreateMap<LoginViewModel, LoginDto>().ReverseMap();
        CreateMap<CreateUserViewModel, UserSaveDto>().ReverseMap();
        CreateMap<CreateUserViewModel, EditUserViewModel>().ReverseMap();
        CreateMap<EditUserViewModel, UserSaveDto>().ReverseMap();
    }
    
}