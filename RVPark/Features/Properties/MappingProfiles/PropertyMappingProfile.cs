using AutoMapper;
using RVPark.Data;
using RVPark.Features.Properties.DTOs;

namespace RVPark.Features.Properties.MappingProfiles;

public class PropertyMappingProfile : Profile
{
    public PropertyMappingProfile()
    {
        CreateMap<Bkg_Property, PropertyDto>()
            .ForMember(dest => dest.PropertyId, opt => opt.MapFrom(src => src.PropertyId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.Zip, opt => opt.MapFrom(src => src.Zip))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.PricePerPeriod, opt => opt.MapFrom(src => src.PricePerPeriod))
            .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled))
            .ForMember(dest => dest.MaxGuestNbr, opt => opt.MapFrom(src => src.MaxGuestNbr))
            .ForMember(dest => dest.MaxRVLength, opt => opt.MapFrom(src => src.MaxRVLength))
            .ForMember(dest => dest.MaxParkWidth, opt => opt.MapFrom(src => src.MaxParkWidth))
            .ForMember(dest => dest.HasPower, opt => opt.MapFrom(src => src.HasPower))
            .ForMember(dest => dest.MaxAmp, opt => opt.MapFrom(src => src.MaxAmp))
            .ForMember(dest => dest.HasWifi, opt => opt.MapFrom(src => src.HasWifi))
            .ForMember(dest => dest.HasSewer, opt => opt.MapFrom(src => src.HasSewer))
            .ForMember(dest => dest.HasPottableWater, opt => opt.MapFrom(src => src.HasPottableWater))
            .ForMember(dest => dest.HasGreyWaterWaste, opt => opt.MapFrom(src => src.HasGreyWaterWaste))
            .ForMember(dest => dest.HasEVCharge, opt => opt.MapFrom(src => src.HasEVCharge))
            .ForMember(dest => dest.EVChargeType, opt => opt.MapFrom(src => src.EVChargeType))
            .ForMember(dest => dest.HasEBikeCharge, opt => opt.MapFrom(src => src.HasEBikeCharge))
            .ForMember(dest => dest.PetSituation, opt => opt.MapFrom(src => src.PetSituation))
            .ForMember(dest => dest.HasBathroom, opt => opt.MapFrom(src => src.HasBathroom))
            .ForMember(dest => dest.HasLaundry, opt => opt.MapFrom(src => src.HasLaundry))
            .ForMember(dest => dest.HasKitchenette, opt => opt.MapFrom(src => src.HasKitchenette))
            .ForMember(dest => dest.HasBedroom, opt => opt.MapFrom(src => src.HasBedroom))
            .ForMember(dest => dest.HasLivingArea, opt => opt.MapFrom(src => src.HasLivingArea))
            .ForMember(dest => dest.HasOffStreetCarPark, opt => opt.MapFrom(src => src.HasOffStreetCarPark))
            .ForMember(dest => dest.KitchenDescription, opt => opt.MapFrom(src => src.KitchenDescription))
            .ForMember(dest => dest.BedroomDescription, opt => opt.MapFrom(src => src.BedroomDescription))
            .ForMember(dest => dest.BathroomDescription, opt => opt.MapFrom(src => src.BathroomDescription))
            .ForMember(dest => dest.LivingAreaDescription, opt => opt.MapFrom(src => src.LivingAreaDescription));
    }
}
