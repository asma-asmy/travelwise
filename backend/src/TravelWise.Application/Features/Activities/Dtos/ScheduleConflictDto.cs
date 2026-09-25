namespace TravelWise.Application.Features.Activities.Dtos;

public class ScheduleConflictDto
{
    public Guid ConflictingScheduledActivityId { get; set; }
    public Guid ConflictingActivityId { get; set; }
    public string ConflictingActivityName { get; set; } = string.Empty;
    public DateOnly ScheduledDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;   // TIME_OVERLAP | OUT_OF_RANGE
}