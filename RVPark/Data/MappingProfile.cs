using AutoMapper;
namespace  RVPark.Data;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApplicationUser, Bkg_User>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.UserFirstName, opt => opt.MapFrom(src => src.FirstNames))
            .ForMember(dest => dest.UserLastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.UserPhone, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.UserAddress, opt => opt.MapFrom(src => src.UserAddress))
            .ForMember(dest => dest.UserCity, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.UserState, opt => opt.MapFrom(src => src.UserState))
            .ForMember(dest => dest.UserZip, opt => opt.MapFrom(src => src.UserZip))
            .ForMember(dest => dest.UserCountry, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.UserStatus, opt => opt.MapFrom(src => src.UserStatus))
            .ForMember(dest => dest.UserPassword, opt => opt.MapFrom(src => src.UserPassword))
            .ReverseMap();
    }
}
