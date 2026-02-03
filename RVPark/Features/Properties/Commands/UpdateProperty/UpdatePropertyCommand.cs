using MediatR;
using RVPark.Features.Properties.DTOs;
using RVPark.Shared;

namespace RVPark.Features.Properties.Commands.UpdateProperty;

public record UpdatePropertyCommand(
    int PropertyId,
    string Name,
    string Description,
    string Address,
    string City,
    string State,
    string Zip,
    string Country,
    decimal PricePerPeriod,
    bool Enabled,
    int MaxGuestNbr,
    decimal MaxRVLength,
    decimal MaxParkWidth,
    bool HasPower,
    int MaxAmp,
    bool HasWifi,
    bool HasSewer,
    bool HasPottableWater,
    bool HasGreyWaterWaste,
    bool HasEVCharge,
    string EVChargeType,
    bool HasEBikeCharge,
    string PetSituation,
    bool HasBathroom,
    bool HasLaundry,
    bool HasKitchenette,
    bool HasBedroom,
    bool HasLivingArea,
    bool HasOffStreetCarPark,
    string KitchenDescription,
    string BedroomDescription,
    string BathroomDescription,
    string LivingAreaDescription
) : IRequest<Result<PropertyDto>>;
