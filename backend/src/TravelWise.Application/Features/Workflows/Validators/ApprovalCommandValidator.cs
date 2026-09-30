using FluentValidation;
using TravelWise.Application.Features.Workflows.Dtos;

namespace TravelWise.Application.Features.Workflows.Validators;

public class ApprovalCommandValidator : AbstractValidator<ApprovalCommandDto>
{
    public ApprovalCommandValidator()
    {
        RuleFor(x => x.Decision)
            .NotEmpty()
            .Must(d => new[] { "APPROVE", "REJECT", "REVISION_REQUIRED" }.Contains(d))
            .WithMessage("Decision must be APPROVE, REJECT, or REVISION_REQUIRED.");

        RuleFor(x => x.Reason)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
    }
}