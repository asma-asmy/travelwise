namespace TravelWise.Application.Features.Activities.Dtos;

public class UpdateScheduledActivityDto
{
    public DateOnly ScheduledDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public string Status { get; set; } = "Planned";
    public string? Notes { get; set; }
}