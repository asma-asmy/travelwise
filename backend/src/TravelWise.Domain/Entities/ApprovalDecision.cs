using TravelWise.Domain.Common;
using TravelWise.Domain.Enums;

namespace TravelWise.Domain.Entities;

public class ApprovalDecision : BaseEntity
{
    public Guid WorkflowRunId { get; set; }
    public WorkflowRun WorkflowRun { get; set; } = null!;

    public Guid DecidedByUserId { get; set; }
    public User DecidedByUser { get; set; } = null!;

    public ApprovalDecisionType Decision { get; set; }
    public string? Reason { get; set; }
    public DateTime DecidedAt { get; set; } = DateTime.UtcNow;
}