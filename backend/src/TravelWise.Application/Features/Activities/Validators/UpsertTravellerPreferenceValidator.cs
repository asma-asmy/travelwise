using FluentValidation;
using TravelWise.Application.Features.Activities.Dtos;

namespace TravelWise.Application.Features.Activities.Validators;

public class UpsertTravellerPreferenceValidator : AbstractValidator<UpsertTravellerPreferenceDto>
{
    public UpsertTravellerPreferenceValidator()
    {
        RuleFor(x => x.Pace).NotEmpty()
            .Must(p => new[] { "Relaxed", "Moderate", "Packed" }.Contains(p))
            .WithMessage("Pace must be Relaxed, Moderate, or Packed.");

        RuleFor(x => x.BudgetTier).NotEmpty()
            .Must(b => new[] { "Budget", "Mid", "Luxury" }.Contains(b))
            .WithMessage("BudgetTier must be Budget, Mid, or Luxury.");

        RuleFor(x => x.Interests)
            .NotNull()
            .Must(i => i.Count <= 20).WithMessage("A maximum of 20 interests is allowed.");

        RuleFor(x => x.Dietary)
            .NotNull()
            .Must(d => d.Count <= 20);
    }
}