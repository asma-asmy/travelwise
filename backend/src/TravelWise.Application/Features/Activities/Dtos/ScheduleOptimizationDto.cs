namespace TravelWise.Application.Features.Activities.Dtos;

public class ScheduleOptimizationDto
{
    public Guid TripId { get; set; }
    public int TotalActivitiesScheduled { get; set; }
    public int TotalMinutesScheduled { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    public int PreferenceMatchCount { get; set; }
    public List<ScheduleConflictDto> Conflicts { get; set; } = new();
    public List<ActivityRecommendationDto> Recommendations { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class ActivityRecommendationDto
{
    public Guid ActivityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public int DurationMinutes { get; set; }
    public double PreferenceScore { get; set; }        // 0..100
    public bool IsIndoor { get; set; }
    public string Location { get; set; } = string.Empty;
}