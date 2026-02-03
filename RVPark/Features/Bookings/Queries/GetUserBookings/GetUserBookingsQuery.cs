using MediatR;
using RVPark.Features.Bookings.DTOs;

namespace RVPark.Features.Bookings.Queries.GetUserBookings;

public record GetUserBookingsQuery(int UserId) : IRequest<List<BookingDto>>;
