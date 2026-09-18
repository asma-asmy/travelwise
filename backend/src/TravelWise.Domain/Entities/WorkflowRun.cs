using TravelWise.Domain.Common;
using TravelWise.Domain.Enums;

namespace TravelWise.Domain.Entities;

public class WorkflowRun : BaseEntity
{
    public Guid TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    public string Objective { get; set; } = string.Empty;
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Created;
    public string? CurrentStep { get; set; }
    public string? PlanJson { get; set; }
    public string? ResultJson { get; set; }
    public string? FailureReason { get; set; }

    public DateTime? CompletedAt { get; set; }

    public ICollection<WorkflowEvent> Events { get; set; } = new List<WorkflowEvent>();
    public ICollection<AgentExecution> Executions { get; set; } = new List<AgentExecution>();
    public ICollection<ValidationResult> Validations { get; set; } = new List<ValidationResult>();
    public ICollection<ApprovalDecision> Approvals { get; set; } = new List<ApprovalDecision>();
}