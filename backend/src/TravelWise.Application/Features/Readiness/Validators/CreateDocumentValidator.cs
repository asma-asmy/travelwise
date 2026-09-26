using FluentValidation;
using TravelWise.Application.Features.Readiness.Dtos;

namespace TravelWise.Application.Features.Readiness.Validators;

public class CreateDocumentValidator : AbstractValidator<CreateDocumentDto>
{
    public CreateDocumentValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Number).NotEmpty().MaximumLength(128);
        RuleFor(x => x.ExpiryDate)
            .GreaterThan(x => x.IssuedDate)
            .When(x => x.IssuedDate.HasValue && x.ExpiryDate.HasValue)
            .WithMessage("Expiry date must be after issued date.");
        RuleFor(x => x.FileRef).MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}