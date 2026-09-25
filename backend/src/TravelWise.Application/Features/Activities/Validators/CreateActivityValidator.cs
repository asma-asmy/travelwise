using FluentValidation;
using TravelWise.Application.Features.Activities.Dtos;
using TravelWise.Domain.Enums;

namespace TravelWise.Application.Features.Activities.Validators;

public class CreateActivityValidator : AbstractValidator<CreateActivityDto>
{
    public CreateActivityValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid activity category.");

        RuleFor(x => x.Description)
            .NotEmpty().MaximumLength(1000);

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0.")
            .LessThanOrEqualTo(1440).WithMessage("Duration cannot exceed 24 hours.");

        RuleFor(x => x.EstimatedCost)
            .GreaterThanOrEqualTo(0).WithMessage("Estimated cost cannot be negative.");

        RuleFor(x => x.Currency)
            .NotEmpty().MaximumLength(8)
            .Matches("^[A-Z]{3,8}$").WithMessage("Currency must be uppercase letters.");

        RuleFor(x => x.Location)
            .NotEmpty().MaximumLength(200);

        RuleFor(x => x.Difficulty)
            .NotEmpty()
            .Must(d => new[] { "Easy", "Moderate", "Hard" }.Contains(d))
            .WithMessage("Difficulty must be Easy, Moderate, or Hard.");
    }
}