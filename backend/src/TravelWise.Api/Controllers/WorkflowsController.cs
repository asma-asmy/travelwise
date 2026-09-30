using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelWise.Application.Features.Budget.Dtos;
using TravelWise.Application.Features.Workflows;
using TravelWise.Application.Features.Workflows.Dtos;

namespace TravelWise.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _workflows;

    public WorkflowsController(IWorkflowService workflows)
    {
        _workflows = workflows;
    }

    [HttpPost("trips/{tripId:guid}/workflows")]
    public async Task<ActionResult<WorkflowResponseDto>> Start(
        Guid tripId,
        [FromBody] StartWorkflowCommandDto command,
        CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        var result = await _workflows.StartAsync(tripId, userId, isStaff, command, ct);
        return Ok(result);
    }

    [HttpGet("workflows/{id:guid}")]
    public async Task<ActionResult<WorkflowResponseDto>> Get(Guid id, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        var result = await _workflows.GetAsync(id, userId, isStaff, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("workflows")]
    public async Task<ActionResult<PagedResult<WorkflowResponseDto>>> List(
        [FromQuery] string? status,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;
        return Ok(await _workflows.ListAsync(status, userId, isStaff, page, pageSize, ct));
    }

    [HttpPost("workflows/{id:guid}/approve")]
    [Authorize(Roles = "TravelOpsStaff,Admin")]
    public async Task<ActionResult<WorkflowResponseDto>> Approve(
        Guid id,
        [FromBody] ApprovalCommandDto command,
        CancellationToken ct)
    {
        command.Decision = "APPROVE";
        return await Decide(id, command, ct);
    }

    [HttpPost("workflows/{id:guid}/reject")]
    [Authorize(Roles = "TravelOpsStaff,Admin")]
    public async Task<ActionResult<WorkflowResponseDto>> Reject(
        Guid id,
        [FromBody] ApprovalCommandDto command,
        CancellationToken ct)
    {
        command.Decision = "REJECT";
        return await Decide(id, command, ct);
    }

    [HttpPost("workflows/{id:guid}/request-revision")]
    [Authorize(Roles = "TravelOpsStaff,Admin")]
    public async Task<ActionResult<WorkflowResponseDto>> RequestRevision(
        Guid id,
        [FromBody] ApprovalCommandDto command,
        CancellationToken ct)
    {
        command.Decision = "REVISION_REQUIRED";
        return await Decide(id, command, ct);
    }

    private async Task<ActionResult<WorkflowResponseDto>> Decide(
        Guid id,
        ApprovalCommandDto command,
        CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        var result = await _workflows.DecideAsync(id, userId, isStaff, command, ct);
        return Ok(result);
    }

    private (Guid userId, bool isStaff) GetUserContext()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(claim, out var id))
            throw new UnauthorizedAccessException("Invalid user claim.");
        var isStaff = User.IsInRole("TravelOpsStaff") || User.IsInRole("Admin");
        return (id, isStaff);
    }
}