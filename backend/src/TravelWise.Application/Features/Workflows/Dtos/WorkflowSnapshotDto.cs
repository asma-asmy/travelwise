using System.Text.Json;
using System.Text.Json.Serialization;

namespace TravelWise.Application.Features.Workflows.Dtos;

public class WorkflowSnapshotDto
{
    [JsonPropertyName("workflow_id")]
    public Guid WorkflowId { get; set; }

    [JsonPropertyName("trip_id")]
    public Guid TripId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("current_step")]
    public string? CurrentStep { get; set; }

    [JsonPropertyName("plan")]
    public JsonElement? Plan { get; set; }

    [JsonPropertyName("result")]
    public JsonElement? Result { get; set; }

    [JsonPropertyName("failure_reason")]
    public string? FailureReason { get; set; }

    [JsonPropertyName("started_at")]
    public DateTime StartedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("agent_executions")]
    public List<AgentExecutionDto> AgentExecutions { get; set; } = new();

    [JsonPropertyName("tool_calls")]
    public List<ToolCallDto> ToolCalls { get; set; } = new();

    [JsonPropertyName("events")]
    public List<WorkflowEventDto> Events { get; set; } = new();
}

public class AgentExecutionDto
{
    [JsonPropertyName("agent_name")]
    public string AgentName { get; set; } = string.Empty;

    [JsonPropertyName("task_name")]
    public string TaskName { get; set; } = string.Empty;

    [JsonPropertyName("output")]
    public JsonElement? Output { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("duration_ms")]
    public long DurationMs { get; set; }
}

public class ToolCallDto
{
    [JsonPropertyName("agent_name")]
    public string AgentName { get; set; } = string.Empty;

    [JsonPropertyName("tool_name")]
    public string ToolName { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public JsonElement? Input { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("duration_ms")]
    public long DurationMs { get; set; }
}

public class WorkflowEventDto
{
    [JsonPropertyName("event_type")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public JsonElement? Payload { get; set; }

    [JsonPropertyName("at")]
    public double At { get; set; }
}