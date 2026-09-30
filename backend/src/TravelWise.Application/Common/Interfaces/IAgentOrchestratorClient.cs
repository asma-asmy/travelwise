using TravelWise.Application.Features.Workflows.Dtos;

namespace TravelWise.Application.Common.Interfaces;

/// <summary>
/// Typed client that wraps the internal AI service. Service-token authenticated.
/// The only way the backend talks to the AI service.
/// </summary>
public interface IAgentOrchestratorClient
{
    Task<WorkflowSnapshotDto> StartAsync(StartWorkflowRequestDto request, CancellationToken ct = default);
    Task<WorkflowSnapshotDto> ResumeAsync(ResumeWorkflowRequestDto request, CancellationToken ct = default);
    Task<WorkflowSnapshotDto?> GetAsync(Guid workflowId, CancellationToken ct = default);
}