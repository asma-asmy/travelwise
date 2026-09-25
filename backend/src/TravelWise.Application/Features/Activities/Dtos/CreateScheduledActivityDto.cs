namespace TravelWise.Application.Features.Activities.Dtos;

public class CreateScheduledActivityDto
{
    public Guid ActivityId { get; set; }
    public DateOnly ScheduledDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public string? Notes { get; set; }
}