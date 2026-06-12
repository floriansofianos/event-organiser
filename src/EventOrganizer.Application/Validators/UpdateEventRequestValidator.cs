using EventOrganizer.Application.DTOs;
using FluentValidation;

namespace EventOrganizer.Application.Validators;

public class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
{
    public UpdateEventRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Event name is required.")
            .MaximumLength(200).WithMessage("Event name must not exceed 200 characters.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Event date is required.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Location)
            .MaximumLength(500).WithMessage("Location must not exceed 500 characters.")
            .When(x => x.Location is not null);
    }
}
