using AutoMapper;
using RVPark.Data;
using RVPark.Features.Bookings.DTOs;

namespace RVPark.Features.Bookings.MappingProfiles;

public class BookingMappingProfile : Profile
{
    public BookingMappingProfile()
    {
        CreateMap<Bkg_Booking, BookingDto>()
            .ForMember(dest => dest.Nights, opt => opt.MapFrom(src => src.Nights));
    }
}
