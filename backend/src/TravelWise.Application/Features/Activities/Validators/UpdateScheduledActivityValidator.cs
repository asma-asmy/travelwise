using FluentValidation;
using TravelWise.Application.Features.Activities.Dtos;

namespace TravelWise.Application.Features.Activities.Validators;

public class UpdateScheduledActivityValidator : AbstractValidator<UpdateScheduledActivityDto>
{
    public UpdateScheduledActivityValidator()
    {
        RuleFor(x => x.ScheduledDate).NotEmpty();
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.Status).NotEmpty()
            .Must(s => new[] { "Planned", "Completed", "Cancelled", "Recovered" }.Contains(s));
    }
}