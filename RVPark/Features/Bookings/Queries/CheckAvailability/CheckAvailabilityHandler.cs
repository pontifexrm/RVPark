using MediatR;
using RVPark.Features.Bookings.Services;
using RVPark.Shared;

namespace RVPark.Features.Bookings.Queries.CheckAvailability;

public class CheckAvailabilityHandler : IRequestHandler<CheckAvailabilityQuery, Result<bool>>
{
    private readonly IBookingEngine _bookingEngine;

    public CheckAvailabilityHandler(IBookingEngine bookingEngine)
    {
        _bookingEngine = bookingEngine;
    }

    public Task<Result<bool>> Handle(CheckAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var result = _bookingEngine.IsAvailable(
            request.DateArrive,
            request.DateDepart,
            request.PropertyId);

        return Task.FromResult(result);
    }
}
