using FluentValidation;

namespace RVPark.Features.Bookings.Commands.EditBooking;

public class EditBookingValidator : AbstractValidator<EditBookingCommand>
{
    public EditBookingValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0)
            .WithMessage("Booking ID must be greater than 0.");

        RuleFor(x => x.PropertyId)
            .GreaterThan(0)
            .WithMessage("Property ID must be greater than 0.");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0.");

        RuleFor(x => x.DateArrive)
            .NotEmpty()
            .WithMessage("Arrival date is required.");

        RuleFor(x => x.DateDepart)
            .NotEmpty()
            .WithMessage("Departure date is required.")
            .GreaterThan(x => x.DateArrive)
            .WithMessage("Departure date must be after arrival date.");

        RuleFor(x => x.TotalPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total price must be 0 or greater.");

        RuleFor(x => x.BookingStatus)
            .NotEmpty()
            .WithMessage("Booking status is required.");
    }
}
