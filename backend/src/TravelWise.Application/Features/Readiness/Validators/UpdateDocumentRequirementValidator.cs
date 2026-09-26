using FluentValidation;
using TravelWise.Application.Features.Readiness.Dtos;

namespace TravelWise.Application.Features.Readiness.Validators;

public class UpdateDocumentRequirementValidator : AbstractValidator<UpdateDocumentRequirementDto>
{
    public UpdateDocumentRequirementValidator()
    {
        RuleFor(x => x.TripType).IsInEnum();
        RuleFor(x => x.DocumentType).IsInEnum();
    }
}