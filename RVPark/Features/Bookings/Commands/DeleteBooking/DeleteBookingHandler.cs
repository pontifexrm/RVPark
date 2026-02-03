using MediatR;
using Microsoft.EntityFrameworkCore;
using RVPark.Data;
using RVPark.Features.Bookings.Services;
using RVPark.Shared;

namespace RVPark.Features.Bookings.Commands.DeleteBooking;

public class DeleteBookingHandler : IRequestHandler<DeleteBookingCommand, Result>
{
    private readonly ApplicationDbContext _context;
    private readonly IBookingEngine _bookingEngine;

    public DeleteBookingHandler(ApplicationDbContext context, IBookingEngine bookingEngine)
    {
        _context = context;
        _bookingEngine = bookingEngine;
    }

    public async Task<Result> Handle(DeleteBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _context.bkg_Bookings!
            .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, cancellationToken);

        if (booking == null)
        {
            return Result.Failure($"Booking with ID {request.BookingId} not found.");
        }

        var success = await _bookingEngine.DeleteBookingAsync(booking);

        if (!success)
        {
            return Result.Failure("Unable to delete booking.");
        }

        return Result.Success();
    }
}
