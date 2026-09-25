using FluentValidation;
using TravelWise.Application.Features.Activities.Dtos;

namespace TravelWise.Application.Features.Activities.Validators;

public class CreateScheduledActivityValidator : AbstractValidator<CreateScheduledActivityDto>
{
    public CreateScheduledActivityValidator()
    {
        RuleFor(x => x.ActivityId).NotEmpty();
        RuleFor(x => x.ScheduledDate).NotEmpty();
        RuleFor(x => x.StartTime).NotEmpty();
    }
}