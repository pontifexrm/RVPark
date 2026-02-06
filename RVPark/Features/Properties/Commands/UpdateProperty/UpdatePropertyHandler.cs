using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RVPark.Data;
using RVPark.Features.Properties.DTOs;
using RVPark.Shared;

namespace RVPark.Features.Properties.Commands.UpdateProperty;

public class UpdatePropertyHandler : IRequestHandler<UpdatePropertyCommand, Result<PropertyDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdatePropertyHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PropertyDto>> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await _context.bkg_Properties
            .FirstOrDefaultAsync(p => p.PropertyId == request.PropertyId, cancellationToken);

        if (property == null)
        {
            return Result<PropertyDto>.Failure($"Property with ID {request.PropertyId} not found.");
        }

        // Update all properties
        property.Name = request.Name;
        property.Description = request.Description;
        property.Address = request.Address;
        property.City = request.City;
        property.State = request.State;
        property.Zip = request.Zip;
        property.Country = request.Country;
        property.PricePerPeriod = request.PricePerPeriod;
        property.Enabled = request.Enabled;
        property.MaxGuestNbr = request.MaxGuestNbr;
        property.MaxRVLength = request.MaxRVLength;
        property.MaxParkWidth = request.MaxParkWidth;
        property.HasPower = request.HasPower;
        property.MaxAmp = request.MaxAmp;
        property.HasWifi = request.HasWifi;
        property.HasSewer = request.HasSewer;
        property.HasPottableWater = request.HasPottableWater;
        property.HasGreyWaterWaste = request.HasGreyWaterWaste;
        property.HasEVCharge = request.HasEVCharge;
        property.EVChargeType = request.EVChargeType;
        property.HasEBikeCharge = request.HasEBikeCharge;
        property.PetSituation = request.PetSituation;
        property.HasBathroom = request.HasBathroom;
        property.HasLaundry = request.HasLaundry;
        property.HasKitchenette = request.HasKitchenette;
        property.HasBedroom = request.HasBedroom;
        property.HasLivingArea = request.HasLivingArea;
        property.HasOffStreetCarPark = request.HasOffStreetCarPark;
        property.KitchenDescription = request.KitchenDescription;
        property.BedroomDescription = request.BedroomDescription;
        property.BathroomDescription = request.BathroomDescription;
        property.LivingAreaDescription = request.LivingAreaDescription;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<PropertyDto>(property);
        return Result<PropertyDto>.Success(dto);
    }
}
