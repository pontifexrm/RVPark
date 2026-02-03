using MediatR;
using RVPark.Shared;

namespace RVPark.Features.Bookings.Commands.DeleteBooking;

public record DeleteBookingCommand(int BookingId) : IRequest<Result>;
