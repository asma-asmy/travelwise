using System.Text.Json;

namespace TravelWise.Application.Features.Workflows.Dtos;

public class WorkflowResponseDto
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public string Objective { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CurrentStep { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }

    public JsonElement? Plan { get; set; }
    public JsonElement? Result { get; set; }

    public List<AgentExecutionResponseDto> AgentExecutions { get; set; } = new();
    public List<ToolCallResponseDto> ToolCalls { get; set; } = new();
    public List<ValidationResultResponseDto> Validations { get; set; } = new();
    public List<ApprovalDecisionResponseDto> Approvals { get; set; } = new();
}

public class AgentExecutionResponseDto
{
    public Guid Id { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long? DurationMs { get; set; }
    public string? OutputJson { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class ToolCallResponseDto
{
    public Guid Id { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public string InputJson { get; set; } = "{}";
    public string? OutputJson { get; set; }
    public string Status { get; set; } = string.Empty;
    public long? DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ValidationResultResponseDto
{
    public Guid Id { get; set; }
    public string ValidatorName { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string DetailsJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}

public class ApprovalDecisionResponseDto
{
    public Guid Id { get; set; }
    public Guid DecidedByUserId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime DecidedAt { get; set; }
}