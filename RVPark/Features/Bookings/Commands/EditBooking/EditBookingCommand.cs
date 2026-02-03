using MediatR;
using RVPark.Features.Bookings.DTOs;
using RVPark.Shared;

namespace RVPark.Features.Bookings.Commands.EditBooking;

public record EditBookingCommand(
    int BookingId,
    int PropertyId,
    int UserId,
    DateTime DateArrive,
    DateTime DateDepart,
    decimal TotalPrice,
    bool Paid,
    string BookingStatus,
    string BookingComments
) : IRequest<Result<BookingDto>>;
