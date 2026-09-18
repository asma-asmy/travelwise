using TravelWise.Domain.Common;
using TravelWise.Domain.Enums;

namespace TravelWise.Domain.Entities;

public class AgentExecution : BaseEntity
{
    public Guid WorkflowRunId { get; set; }
    public WorkflowRun WorkflowRun { get; set; } = null!;

    public string AgentName { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;

    public string InputJson { get; set; } = "{}";
    public string? OutputJson { get; set; }

    public AgentExecutionStatus Status { get; set; } = AgentExecutionStatus.Pending;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? DurationMs { get; set; }
    public string? ErrorMessage { get; set; }

    public ICollection<ToolCall> ToolCalls { get; set; } = new List<ToolCall>();
}