using MediatR;
using Microsoft.EntityFrameworkCore;
using RVPark.Data;
using RVPark.Features.Bookings.DTOs;

namespace RVPark.Features.Bookings.Queries.GetUserBookings;

public class GetUserBookingsHandler : IRequestHandler<GetUserBookingsQuery, List<BookingDto>>
{
    private readonly ApplicationDbContext _context;

    public GetUserBookingsHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BookingDto>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
    {
        // Use AsNoTracking to always get fresh data from database
        var query = _context.bkg_Bookings.AsNoTracking();

        // Filter by user
        query = query.Where(b => b.UserId == request.UserId);

        // Filter closed bookings unless requested
        if (!request.IncludeClosedBookings)
        {
            query = query.Where(b => b.BookingStatus != "Closed");
        }

        // Include related data if requested
        if (request.IncludeRelatedData)
        {
            query = query
                .Include(b => b.Bkg_User)
                .Include(b => b.Bkg_Property);
        }

        var bookings = await query
            .OrderBy(b => b.DateArrive)
            .ToListAsync(cancellationToken);

        // Map to DTOs with related data
        return bookings.Select(b => new BookingDto(
            BookingId: b.BookingId,
            PropertyId: b.PropertyId,
            UserId: b.UserId,
            DateArrive: b.DateArrive,
            DateDepart: b.DateDepart,
            TotalPrice: b.TotalPrice,
            Paid: b.Paid,
            BookingStatus: b.BookingStatus,
            BookingComments: b.BookingComments,
            CreatedDte: b.CreatedDte,
            UpdatedDte: b.UpdatedDte,
            Nights: b.Nights,
            PropertyName: b.Bkg_Property?.Name,
            UserFirstName: b.Bkg_User?.UserFirstName,
            UserLastName: b.Bkg_User?.UserLastName,
            UserName: b.Bkg_User?.UserName,
            UserEmail: b.Bkg_User?.UserEmail,
            UserPhone: b.Bkg_User?.UserPhone
        )).ToList();
    }
}
