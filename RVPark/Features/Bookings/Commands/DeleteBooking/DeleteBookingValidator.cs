using FluentValidation;

namespace RVPark.Features.Bookings.Commands.DeleteBooking;

public class DeleteBookingValidator : AbstractValidator<DeleteBookingCommand>
{
    public DeleteBookingValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0)
            .WithMessage("Booking ID must be greater than 0.");
    }
}
