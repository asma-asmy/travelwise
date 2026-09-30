using FluentValidation;
using TravelWise.Application.Features.Workflows.Dtos;

namespace TravelWise.Application.Features.Workflows.Validators;

public class StartWorkflowCommandValidator : AbstractValidator<StartWorkflowCommandDto>
{
    public StartWorkflowCommandValidator()
    {
        RuleFor(x => x.Objective)
            .NotEmpty().WithMessage("Objective is required.")
            .MinimumLength(5).WithMessage("Objective must be at least 5 characters.")
            .MaximumLength(1000);
    }
}