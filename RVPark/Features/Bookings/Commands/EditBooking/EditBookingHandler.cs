using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RVPark.Data;
using RVPark.Features.Bookings.DTOs;
using RVPark.Features.Bookings.Services;
using RVPark.Shared;

namespace RVPark.Features.Bookings.Commands.EditBooking;

public class EditBookingHandler : IRequestHandler<EditBookingCommand, Result<BookingDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IBookingEngine _bookingEngine;
    private readonly IMapper _mapper;

    public EditBookingHandler(ApplicationDbContext context, IBookingEngine bookingEngine, IMapper mapper)
    {
        _context = context;
        _bookingEngine = bookingEngine;
        _mapper = mapper;
    }

    public async Task<Result<BookingDto>> Handle(EditBookingCommand request, CancellationToken cancellationToken)
    {
        var currentBooking = await _context.bkg_Bookings!
            .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, cancellationToken);

        if (currentBooking == null)
        {
            return Result<BookingDto>.Failure($"Booking with ID {request.BookingId} not found.");
        }

        var newBooking = new Bkg_Booking
        {
            BookingId = request.BookingId,
            PropertyId = request.PropertyId,
            UserId = request.UserId,
            DateArrive = request.DateArrive,
            DateDepart = request.DateDepart,
            TotalPrice = request.TotalPrice,
            Paid = request.Paid,
            BookingStatus = request.BookingStatus,
            BookingComments = request.BookingComments,
            CreatedDte = currentBooking.CreatedDte,
            UpdatedDte = DateTime.UtcNow
        };

        var success = await _bookingEngine.EditBookingAsync(currentBooking, newBooking);

        if (!success)
        {
            return Result<BookingDto>.Failure("Unable to edit booking. The requested dates may not be available.");
        }

        var dto = _mapper.Map<BookingDto>(newBooking);
        return Result<BookingDto>.Success(dto);
    }
}
