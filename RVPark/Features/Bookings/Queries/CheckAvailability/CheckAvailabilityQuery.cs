using MediatR;
using RVPark.Shared;

namespace RVPark.Features.Bookings.Queries.CheckAvailability;

public record CheckAvailabilityQuery(
    int PropertyId,
    DateTime DateArrive,
    DateTime DateDepart
) : IRequest<Result<bool>>;
