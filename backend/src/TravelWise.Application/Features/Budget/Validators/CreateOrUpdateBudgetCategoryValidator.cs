using FluentValidation;
using TravelWise.Application.Features.Budget.Dtos;

namespace TravelWise.Application.Features.Budget.Validators;

public class CreateOrUpdateBudgetCategoryValidator : AbstractValidator<CreateOrUpdateBudgetCategoryDto>
{
    public CreateOrUpdateBudgetCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LimitAmount)
            .GreaterThan(0).WithMessage("Category limit must be greater than 0.");
    }
}