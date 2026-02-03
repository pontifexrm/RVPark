using MediatR;
using RVPark.Data;
using RVPark.Features.Bookings.Services;

namespace RVPark.Features.Bookings.Queries.CheckAvailability;

public class CheckAvailabilityHandler : IRequestHandler<CheckAvailabilityQuery, bool>
{
    private readonly IBookingEngine _bookingEngine;

    public CheckAvailabilityHandler(IBookingEngine bookingEngine)
    {
        _bookingEngine = bookingEngine;
    }

    public Task<bool> Handle(CheckAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var isAvailable = _bookingEngine.IsAvailable(
            request.DateArrive,
            request.DateDepart,
            request.PropertyId);

        return Task.FromResult(isAvailable);
    }
}
