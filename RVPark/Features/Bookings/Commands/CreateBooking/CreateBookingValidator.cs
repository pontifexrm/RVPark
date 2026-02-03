using FluentValidation;

namespace RVPark.Features.Bookings.Commands.CreateBooking;

public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.PropertyId)
            .GreaterThan(0)
            .WithMessage("Property ID must be greater than 0.");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0.");

        RuleFor(x => x.DateArrive)
            .NotEmpty()
            .WithMessage("Arrival date is required.")
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Arrival date cannot be in the past.");

        RuleFor(x => x.DateDepart)
            .NotEmpty()
            .WithMessage("Departure date is required.")
            .GreaterThan(x => x.DateArrive)
            .WithMessage("Departure date must be after arrival date.");

        RuleFor(x => x.TotalPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total price must be 0 or greater.");
    }
}
