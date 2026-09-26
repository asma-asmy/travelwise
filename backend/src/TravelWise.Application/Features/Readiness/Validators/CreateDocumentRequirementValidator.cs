using FluentValidation;
using TravelWise.Application.Features.Readiness.Dtos;

namespace TravelWise.Application.Features.Readiness.Validators;

public class CreateDocumentRequirementValidator : AbstractValidator<CreateDocumentRequirementDto>
{
    public CreateDocumentRequirementValidator()
    {
        RuleFor(x => x.Destination).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TripType).IsInEnum();
        RuleFor(x => x.DocumentType).IsInEnum();
    }
}