using MediatR;
using RVPark.Features.Bookings.DTOs;
using RVPark.Shared;

namespace RVPark.Features.Bookings.Queries.GetBookingById;

public record GetBookingByIdQuery(int BookingId) : IRequest<Result<BookingDto>>;
