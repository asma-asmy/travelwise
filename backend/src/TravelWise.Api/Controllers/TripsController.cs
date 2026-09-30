using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelWise.Application.Features.Trips;
using TravelWise.Application.Features.Trips.Dtos;

namespace TravelWise.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class TripsController : ControllerBase
{
    private readonly ITripService _trips;

    public TripsController(ITripService trips)
    {
        _trips = trips;
    }

    /// <summary>List trips owned by the current user.</summary>
    [HttpGet("trips")]
    public async Task<ActionResult<List<TripDto>>> ListMine(CancellationToken ct)
    {
        var userId = GetUserId();
        return Ok(await _trips.ListMyTripsAsync(userId, ct));
    }

    /// <summary>List all trips (cross-user). Staff/Admin only.</summary>
    [HttpGet("admin/trips")]
    [Authorize(Roles = "TravelOpsStaff,Admin")]
    public async Task<ActionResult<List<TripDto>>> ListAll(CancellationToken ct)
    {
        return Ok(await _trips.ListAllTripsAsync(ct));
    }

    /// <summary>Single trip by ID. Staff can access any; travellers only their own.</summary>
    [HttpGet("trips/{id:guid}")]
    public async Task<ActionResult<TripDto>> Get(Guid id, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        return Ok(await _trips.GetAsync(id, userId, isStaff, ct));
    }

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