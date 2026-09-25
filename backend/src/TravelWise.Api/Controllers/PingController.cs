using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelWise.Api.Controllers;

[ApiController]
[Route("api/v1/ping")]
public class PingController : ControllerBase
{
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult PublicPing() => Ok(new { message = "public ok", atUtc = DateTime.UtcNow });

    [HttpGet("traveller")]
    [Authorize(Roles = "Traveller,TravelOpsStaff,Admin")]
    public IActionResult TravellerPing() => Ok(new { message = "traveller ok", atUtc = DateTime.UtcNow });

    [HttpGet("staff")]
    [Authorize(Roles = "TravelOpsStaff,Admin")]
    public IActionResult StaffPing() => Ok(new { message = "staff ok", atUtc = DateTime.UtcNow });

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminPing() => Ok(new { message = "admin ok", atUtc = DateTime.UtcNow });
}