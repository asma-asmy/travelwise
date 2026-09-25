using FluentValidation;
using TravelWise.Application.Features.Budget.Dtos;

namespace TravelWise.Application.Features.Budget.Validators;

public class CreateOrUpdateBudgetValidator : AbstractValidator<CreateOrUpdateBudgetDto>
{
    public CreateOrUpdateBudgetValidator()
    {
        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Budget total must be greater than 0.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(8)
            .Matches("^[A-Z]{3,8}$").WithMessage("Currency must be uppercase letters (e.g. LKR).");
    }
}