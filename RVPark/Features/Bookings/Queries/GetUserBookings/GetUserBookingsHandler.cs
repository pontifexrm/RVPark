using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RVPark.Data;
using RVPark.Features.Bookings.DTOs;

namespace RVPark.Features.Bookings.Queries.GetUserBookings;

public class GetUserBookingsHandler : IRequestHandler<GetUserBookingsQuery, List<BookingDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserBookingsHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<BookingDto>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _context.bkg_Bookings!
            .Where(b => b.UserId == request.UserId)
            .OrderByDescending(b => b.DateArrive)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<BookingDto>>(bookings);
    }
}
