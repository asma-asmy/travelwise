using FluentValidation;
using TravelWise.Application.Features.Activities.Dtos;

namespace TravelWise.Application.Features.Activities.Validators;

public class UpdateActivityValidator : AbstractValidator<UpdateActivityDto>
{
    public UpdateActivityValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.DurationMinutes).GreaterThan(0).LessThanOrEqualTo(1440);
        RuleFor(x => x.EstimatedCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(8).Matches("^[A-Z]{3,8}$");
        RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Difficulty).NotEmpty()
            .Must(d => new[] { "Easy", "Moderate", "Hard" }.Contains(d));
    }
}