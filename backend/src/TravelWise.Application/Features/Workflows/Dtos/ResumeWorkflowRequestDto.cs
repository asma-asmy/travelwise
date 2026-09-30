using System.Text.Json.Serialization;

namespace TravelWise.Application.Features.Workflows.Dtos;

public class ResumeWorkflowRequestDto
{
    [JsonPropertyName("workflow_id")]
    public Guid WorkflowId { get; set; }

    [JsonPropertyName("decision")]
    public string Decision { get; set; } = string.Empty;   // APPROVE | REJECT | REVISION_REQUIRED

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("decided_by_user_id")]
    public Guid DecidedByUserId { get; set; }
}