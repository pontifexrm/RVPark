using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RVPark.Data;
using RVPark.Features.Bookings.DTOs;
using RVPark.Shared;

namespace RVPark.Features.Bookings.Queries.GetBookingById;

public class GetBookingByIdHandler : IRequestHandler<GetBookingByIdQuery, Result<BookingDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBookingByIdHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<BookingDto>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await _context.bkg_Bookings
            .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, cancellationToken);

        if (booking == null)
        {
            return Result<BookingDto>.Failure($"Booking with ID {request.BookingId} not found.");
        }

        var dto = _mapper.Map<BookingDto>(booking);
        return Result<BookingDto>.Success(dto);
    }
}
