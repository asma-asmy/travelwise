using FluentValidation;
using TravelWise.Application.Features.Budget.Dtos;

namespace TravelWise.Application.Features.Budget.Validators;

public class CreateExpenseValidator : AbstractValidator<CreateExpenseDto>
{
    public CreateExpenseValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Expense amount must be greater than 0.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(300);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(8);

        RuleFor(x => x.SpentAt)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .When(x => x.SpentAt.HasValue)
            .WithMessage("Spent date cannot be in the future.");
    }
}