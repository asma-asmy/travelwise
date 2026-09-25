using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelWise.Application.Features.Budget.Dtos;
using TravelWise.Application.Features.Risk;
using TravelWise.Application.Features.Risk.Dtos;

namespace TravelWise.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class RiskController : ControllerBase
{
    private readonly IRiskService _risk;

    public RiskController(IRiskService risk)
    {
        _risk = risk;
    }

    // ---------------- Alerts (admin) ----------------

    [HttpGet("alerts")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<SafetyAlertDto>>> ListAlerts([FromQuery] SafetyAlertQueryDto query, CancellationToken ct)
    {
        return Ok(await _risk.ListAlertsAsync(query, ct));
    }

    [HttpPost("alerts")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SafetyAlertDto>> CreateAlert([FromBody] CreateSafetyAlertDto dto, CancellationToken ct)
    {
        var result = await _risk.CreateAlertAsync(dto, ct);
        return CreatedAtAction(nameof(ListAlerts), new { }, result);
    }

    [HttpPut("alerts/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SafetyAlertDto>> UpdateAlert(Guid id, [FromBody] UpdateSafetyAlertDto dto, CancellationToken ct)
    {
        return Ok(await _risk.UpdateAlertAsync(id, dto, ct));
    }

    [HttpDelete("alerts/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAlert(Guid id, CancellationToken ct)
    {
        await _risk.DeleteAlertAsync(id, ct);
        return NoContent();
    }

    // ---------------- Destination risk ----------------

    [HttpGet("destinations/{destination}/risk")]
    [AllowAnonymous]
    public async Task<ActionResult<DestinationRiskDto>> GetDestinationRisk(string destination, CancellationToken ct)
    {
        return Ok(await _risk.GetDestinationRiskAsync(destination, ct));
    }

    // ---------------- Weather (proxied) ----------------

    [HttpGet("destinations/{destination}/weather")]
    [AllowAnonymous]
    public async Task<ActionResult<WeatherDto?>> GetWeather(string destination, [FromQuery] DateOnly date, CancellationToken ct)
    {
        var result = await _risk.GetWeatherAsync(destination, date, ct);
        return result is null ? NoContent() : Ok(result);
    }

    // ---------------- Safety info ----------------

    [HttpGet("destinations/{destination}/safety")]
    [AllowAnonymous]
    public async Task<ActionResult<SafetyInformationDto>> GetSafetyInfo(string destination, CancellationToken ct)
    {
        return Ok(await _risk.GetSafetyInformationAsync(destination, ct));
    }

    // ---------------- Trip risk (business operations) ----------------

    [HttpGet("trips/{tripId:guid}/risk")]
    public async Task<ActionResult<RiskAssessmentDto?>> GetLatest(Guid tripId, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        var result = await _risk.GetLatestAssessmentAsync(tripId, userId, isStaff, ct);
        return result is null ? NoContent() : Ok(result);
    }

    [HttpPost("trips/{tripId:guid}/risk/assess")]
    public async Task<ActionResult<RiskAssessmentDto>> Assess(Guid tripId, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        return Ok(await _risk.AssessTripAsync(tripId, userId, isStaff, ct));
    }

    // ---------------- Helpers ----------------

    private (Guid userId, bool isStaff) GetUserContext()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(claim, out var id))
            throw new UnauthorizedAccessException("Invalid user claim.");
        var isStaff = User.IsInRole("TravelOpsStaff") || User.IsInRole("Admin");
        return (id, isStaff);
    }
}