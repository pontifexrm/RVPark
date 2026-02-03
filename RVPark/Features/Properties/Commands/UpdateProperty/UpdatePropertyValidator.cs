using FluentValidation;

namespace RVPark.Features.Properties.Commands.UpdateProperty;

public class UpdatePropertyValidator : AbstractValidator<UpdatePropertyCommand>
{
    public UpdatePropertyValidator()
    {
        RuleFor(x => x.PropertyId)
            .GreaterThan(0)
            .WithMessage("Property ID must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Property name is required.")
            .MaximumLength(100)
            .WithMessage("Property name cannot exceed 100 characters.");

        RuleFor(x => x.PricePerPeriod)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price per period must be 0 or greater.");

        RuleFor(x => x.MaxGuestNbr)
            .GreaterThan(0)
            .WithMessage("Maximum guest number must be at least 1.");

        RuleFor(x => x.MaxRVLength)
            .GreaterThan(0)
            .WithMessage("Maximum RV length must be greater than 0.");

        RuleFor(x => x.MaxParkWidth)
            .GreaterThan(0)
            .WithMessage("Maximum park width must be greater than 0.");
    }
}
