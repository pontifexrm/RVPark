using MediatR;
using RVPark.Features.Bookings.DTOs;
using RVPark.Shared;

namespace RVPark.Features.Bookings.Commands.CreateBooking;

public record CreateBookingCommand(
    int PropertyId,
    int UserId,
    DateTime DateArrive,
    DateTime DateDepart,
    decimal TotalPrice,
    string BookingComments = ""
) : IRequest<Result<BookingDto>>;
