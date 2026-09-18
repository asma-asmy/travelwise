using TravelWise.Domain.Common;
using TravelWise.Domain.Enums;

namespace TravelWise.Domain.Entities;

public class ToolCall : BaseEntity
{
    public Guid AgentExecutionId { get; set; }
    public AgentExecution AgentExecution { get; set; } = null!;

    public string ToolName { get; set; } = string.Empty;
    public string InputJson { get; set; } = "{}";
    public string? OutputJson { get; set; }

    public AgentExecutionStatus Status { get; set; } = AgentExecutionStatus.Pending;
    public long? DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
}