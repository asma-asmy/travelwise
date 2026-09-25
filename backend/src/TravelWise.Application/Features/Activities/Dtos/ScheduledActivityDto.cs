namespace TravelWise.Application.Features.Activities.Dtos;

public class ScheduledActivityDto
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public Guid ActivityId { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityCategory { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsIndoor { get; set; }
    public int DurationMinutes { get; set; }
    public decimal EstimatedCost { get; set; }
    public DateOnly ScheduledDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}