using TravelWise.Application.Features.Budget.Dtos;
using TravelWise.Application.Features.Workflows.Dtos;

namespace TravelWise.Application.Features.Workflows;

public interface IWorkflowService
{
    Task<WorkflowResponseDto> StartAsync(
        Guid tripId,
        Guid userId,
        bool isStaff,
        StartWorkflowCommandDto command,
        CancellationToken ct = default);

    Task<WorkflowResponseDto?> GetAsync(
        Guid workflowId,
        Guid userId,
        bool isStaff,
        CancellationToken ct = default);

    Task<PagedResult<WorkflowResponseDto>> ListAsync(
        string? status,
        Guid userId,
        bool isStaff,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<WorkflowResponseDto> DecideAsync(
        Guid workflowId,
        Guid userId,
        bool isStaff,
        ApprovalCommandDto command,
        CancellationToken ct = default);
}