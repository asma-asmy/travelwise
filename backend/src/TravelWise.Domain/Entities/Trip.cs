using TravelWise.Domain.Common;
using TravelWise.Domain.Enums;

namespace TravelWise.Domain.Entities;

public class Trip : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string? Origin { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public string Currency { get; set; } = "LKR";
    public TripType TripType { get; set; } = TripType.Leisure;
    public TripStatus Status { get; set; } = TripStatus.Draft;

    public int TravellerCount { get; set; } = 1;

    // Navigation
    public Budget? Budget { get; set; }
    public ICollection<ScheduledActivity> ScheduledActivities { get; set; } = new List<ScheduledActivity>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<RiskAssessment> RiskAssessments { get; set; } = new List<RiskAssessment>();
    public ICollection<ReadinessAssessment> ReadinessAssessments { get; set; } = new List<ReadinessAssessment>();
    public ICollection<WorkflowRun> WorkflowRuns { get; set; } = new List<WorkflowRun>();
}