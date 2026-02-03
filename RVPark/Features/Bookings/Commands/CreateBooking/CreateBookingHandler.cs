using AutoMapper;
using MediatR;
using RVPark.Data;
using RVPark.Features.Bookings.DTOs;
using RVPark.Features.Bookings.Services;
using RVPark.Shared;

namespace RVPark.Features.Bookings.Commands.CreateBooking;

public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, Result<BookingDto>>
{
    private readonly IBookingEngine _bookingEngine;
    private readonly IMapper _mapper;

    public CreateBookingHandler(IBookingEngine bookingEngine, IMapper mapper)
    {
        _bookingEngine = bookingEngine;
        _mapper = mapper;
    }

    public async Task<Result<BookingDto>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = new Bkg_Booking
        {
            PropertyId = request.PropertyId,
            UserId = request.UserId,
            DateArrive = request.DateArrive,
            DateDepart = request.DateDepart,
            TotalPrice = request.TotalPrice,
            BookingComments = request.BookingComments,
            CreatedDte = DateTime.UtcNow,
            UpdatedDte = DateTime.UtcNow
        };

        var success = await _bookingEngine.CreateBookingAsync(booking);

        if (!success)
        {
            return Result<BookingDto>.Failure("Unable to create booking. The requested dates may not be available.");
        }

        var dto = _mapper.Map<BookingDto>(booking);
        return Result<BookingDto>.Success(dto);
    }
}
