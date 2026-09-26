using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelWise.Application.Features.Budget.Dtos;
using TravelWise.Application.Features.Readiness;
using TravelWise.Application.Features.Readiness.Dtos;

namespace TravelWise.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class ReadinessController : ControllerBase
{
    private readonly IReadinessService _readiness;

    public ReadinessController(IReadinessService readiness)
    {
        _readiness = readiness;
    }

    // ---------------- Documents (mine) ----------------

    [HttpGet("users/me/documents")]
    public async Task<ActionResult<PagedResult<DocumentDto>>> ListMyDocuments([FromQuery] DocumentQueryDto query, CancellationToken ct)
    {
        var userId = GetUserId();
        return Ok(await _readiness.ListMyDocumentsAsync(userId, query, ct));
    }

    [HttpGet("users/me/documents/{id:guid}")]
    public async Task<ActionResult<DocumentDto>> GetMyDocument(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        return Ok(await _readiness.GetMyDocumentAsync(userId, id, ct));
    }

    [HttpPost("users/me/documents")]
    public async Task<ActionResult<DocumentDto>> AddMyDocument([FromBody] CreateDocumentDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _readiness.AddMyDocumentAsync(userId, dto, ct);
        return CreatedAtAction(nameof(GetMyDocument), new { id = result.Id }, result);
    }

    [HttpPut("users/me/documents/{id:guid}")]
    public async Task<ActionResult<DocumentDto>> UpdateMyDocument(Guid id, [FromBody] UpdateDocumentDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        return Ok(await _readiness.UpdateMyDocumentAsync(userId, id, dto, ct));
    }

    [HttpDelete("users/me/documents/{id:guid}")]
    public async Task<IActionResult> DeleteMyDocument(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        await _readiness.DeleteMyDocumentAsync(userId, id, ct);
        return NoContent();
    }

    // ---------------- Requirements (admin) ----------------

    [HttpGet("destinations/{destination}/requirements")]
    [AllowAnonymous]
    public async Task<ActionResult<List<DocumentRequirementDto>>> ListRequirements(string destination, CancellationToken ct)
    {
        return Ok(await _readiness.ListRequirementsAsync(destination, ct));
    }

    [HttpGet("requirements")]
    [AllowAnonymous]
    public async Task<ActionResult<List<DocumentRequirementDto>>> ListAllRequirements([FromQuery] string? destination, CancellationToken ct)
    {
        return Ok(await _readiness.ListRequirementsAsync(destination, ct));
    }

    [HttpPost("requirements")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DocumentRequirementDto>> CreateRequirement([FromBody] CreateDocumentRequirementDto dto, CancellationToken ct)
    {
        var result = await _readiness.CreateRequirementAsync(dto, ct);
        return CreatedAtAction(nameof(ListAllRequirements), new { }, result);
    }

    [HttpPut("requirements/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DocumentRequirementDto>> UpdateRequirement(Guid id, [FromBody] UpdateDocumentRequirementDto dto, CancellationToken ct)
    {
        return Ok(await _readiness.UpdateRequirementAsync(id, dto, ct));
    }

    [HttpDelete("requirements/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRequirement(Guid id, CancellationToken ct)
    {
        await _readiness.DeleteRequirementAsync(id, ct);
        return NoContent();
    }

    // ---------------- Trip readiness (business operations) ----------------

    [HttpGet("trips/{tripId:guid}/readiness")]
    public async Task<ActionResult<ReadinessAssessmentDto?>> GetLatest(Guid tripId, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        var result = await _readiness.GetLatestAssessmentAsync(tripId, userId, isStaff, ct);
        return result is null ? NoContent() : Ok(result);
    }

    [HttpPost("trips/{tripId:guid}/readiness/assess")]
    public async Task<ActionResult<ReadinessAssessmentDto>> Assess(Guid tripId, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        return Ok(await _readiness.AssessTripAsync(tripId, userId, isStaff, ct));
    }

    // ---------------- Helpers ----------------

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(claim, out var id))
            throw new UnauthorizedAccessException("Invalid user claim.");
        return id;
    }

    private (Guid userId, bool isStaff) GetUserContext()
    {
        var userId = GetUserId();
        var isStaff = User.IsInRole("TravelOpsStaff") || User.IsInRole("Admin");
        return (userId, isStaff);
    }
}