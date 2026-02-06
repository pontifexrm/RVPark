using MediatR;
using RVPark.Features.Bookings.DTOs;

namespace RVPark.Features.Bookings.Queries.GetUserBookings;

/// <summary>
/// Query to get bookings for a specific user.
/// </summary>
/// <param name="UserId">The booking user ID (Bkg_User.UserId)</param>
/// <param name="IncludeClosedBookings">If true, includes closed bookings; otherwise only active bookings</param>
/// <param name="IncludeRelatedData">If true, includes property and user details in the response</param>
public record GetUserBookingsQuery(
    int UserId,
    bool IncludeClosedBookings = false,
    bool IncludeRelatedData = true
) : IRequest<List<BookingDto>>;
