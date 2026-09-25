using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelWise.Application.Features.Activities;
using TravelWise.Application.Features.Activities.Dtos;
using TravelWise.Application.Features.Budget.Dtos;

namespace TravelWise.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activities;

    public ActivitiesController(IActivityService activities)
    {
        _activities = activities;
    }

    // ---------------- Catalogue ----------------

    [HttpGet("activities")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ActivityDto>>> Search([FromQuery] ActivityQueryDto query, CancellationToken ct)
    {
        var result = await _activities.SearchAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("activities/{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ActivityDto>> Get(Guid id, CancellationToken ct)
    {
        return Ok(await _activities.GetAsync(id, ct));
    }

    [HttpPost("activities")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ActivityDto>> Create([FromBody] CreateActivityDto dto, CancellationToken ct)
    {
        var result = await _activities.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("activities/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ActivityDto>> Update(Guid id, [FromBody] UpdateActivityDto dto, CancellationToken ct)
    {
        return Ok(await _activities.UpdateAsync(id, dto, ct));
    }

    [HttpDelete("activities/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _activities.DeleteAsync(id, ct);
        return NoContent();
    }

    // ---------------- Preferences ----------------

    [HttpGet("users/me/preferences")]
    public async Task<ActionResult<TravellerPreferenceDto?>> GetMyPreferences(CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _activities.GetMyPreferencesAsync(userId, ct);
        return result is null ? NoContent() : Ok(result);
    }

    [HttpPut("users/me/preferences")]
    public async Task<ActionResult<TravellerPreferenceDto>> UpsertMyPreferences([FromBody] UpsertTravellerPreferenceDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        return Ok(await _activities.UpsertMyPreferencesAsync(userId, dto, ct));
    }

    // ---------------- Scheduling (trip-scoped) ----------------

    [HttpGet("trips/{tripId:guid}/schedule")]
    public async Task<ActionResult<List<ScheduledActivityDto>>> ListScheduled(Guid tripId, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        return Ok(await _activities.ListScheduledAsync(tripId, userId, isStaff, ct));
    }

    [HttpPost("trips/{tripId:guid}/schedule")]
    public async Task<ActionResult<ScheduledActivityDto>> Schedule(Guid tripId, [FromBody] CreateScheduledActivityDto dto, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        var result = await _activities.ScheduleAsync(tripId, userId, isStaff, dto, ct);
        return CreatedAtAction(nameof(ListScheduled), new { tripId }, result);
    }

    [HttpPut("trips/{tripId:guid}/schedule/{scheduledId:guid}")]
    public async Task<ActionResult<ScheduledActivityDto>> UpdateScheduled(Guid tripId, Guid scheduledId, [FromBody] UpdateScheduledActivityDto dto, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        return Ok(await _activities.UpdateScheduledAsync(tripId, scheduledId, userId, isStaff, dto, ct));
    }

    [HttpDelete("trips/{tripId:guid}/schedule/{scheduledId:guid}")]
    public async Task<IActionResult> DeleteScheduled(Guid tripId, Guid scheduledId, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        await _activities.DeleteScheduledAsync(tripId, scheduledId, userId, isStaff, ct);
        return NoContent();
    }

    // ---------------- Business Operations ----------------

    [HttpGet("trips/{tripId:guid}/schedule/conflicts")]
    public async Task<ActionResult<List<ScheduleConflictDto>>> GetConflicts(Guid tripId, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        return Ok(await _activities.GetConflictsAsync(tripId, userId, isStaff, ct));
    }

    [HttpPost("trips/{tripId:guid}/schedule/optimize")]
    public async Task<ActionResult<ScheduleOptimizationDto>> Optimize(Guid tripId, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        return Ok(await _activities.OptimizeAsync(tripId, userId, isStaff, ct));
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