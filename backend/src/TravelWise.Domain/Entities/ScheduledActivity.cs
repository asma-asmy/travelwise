using TravelWise.Domain.Common;

namespace TravelWise.Domain.Entities;

public class ScheduledActivity : BaseEntity
{
    public Guid TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    public Guid ActivityId { get; set; }
    public Activity Activity { get; set; } = null!;

    public DateOnly ScheduledDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public string Status { get; set; } = "Planned";   // Planned | Completed | Cancelled | Recovered
    public string? Notes { get; set; }
}