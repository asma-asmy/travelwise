using System.Text.Json.Serialization;

namespace TravelWise.Application.Features.Workflows.Dtos;

public class StartWorkflowRequestDto
{
    [JsonPropertyName("workflow_id")]
    public Guid WorkflowId { get; set; }

    [JsonPropertyName("trip_id")]
    public Guid TripId { get; set; }

    [JsonPropertyName("triggered_by_user_id")]
    public Guid TriggeredByUserId { get; set; }

    [JsonPropertyName("objective")]
    public string Objective { get; set; } = string.Empty;

    [JsonPropertyName("disruption_hint")]
    public string? DisruptionHint { get; set; }
}