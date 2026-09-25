using FluentValidation;
using TravelWise.Application.Features.Risk.Dtos;

namespace TravelWise.Application.Features.Risk.Validators;

public class UpdateSafetyAlertValidator : AbstractValidator<UpdateSafetyAlertDto>
{
    public UpdateSafetyAlertValidator()
    {
        RuleFor(x => x.Category).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Severity).IsInEnum();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ValidTo).GreaterThan(x => x.ValidFrom);
    }
}