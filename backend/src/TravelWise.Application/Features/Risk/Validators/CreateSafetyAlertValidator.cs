using FluentValidation;
using TravelWise.Application.Features.Risk.Dtos;

namespace TravelWise.Application.Features.Risk.Validators;

public class CreateSafetyAlertValidator : AbstractValidator<CreateSafetyAlertDto>
{
    public CreateSafetyAlertValidator()
    {
        RuleFor(x => x.Destination).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(64)
            .Must(c => new[] { "Weather", "Health", "Transport", "Safety", "Other" }.Contains(c, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Category must be one of: Weather, Health, Transport, Safety, Other.");
        RuleFor(x => x.Severity).IsInEnum();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ValidFrom).NotEmpty();
        RuleFor(x => x.ValidTo).NotEmpty()
            .GreaterThan(x => x.ValidFrom)
            .WithMessage("ValidTo must be after ValidFrom.");
    }
}