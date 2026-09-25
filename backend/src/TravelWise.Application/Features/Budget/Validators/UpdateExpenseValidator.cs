using FluentValidation;
using TravelWise.Application.Features.Budget.Dtos;

namespace TravelWise.Application.Features.Budget.Validators;

public class UpdateExpenseValidator : AbstractValidator<UpdateExpenseDto>
{
    public UpdateExpenseValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(8);
        RuleFor(x => x.SpentAt).LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5));
    }
}