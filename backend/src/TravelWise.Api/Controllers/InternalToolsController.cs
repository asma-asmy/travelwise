using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelWise.Domain.Enums;
using TravelWise.Infrastructure.Persistence;

namespace TravelWise.Api.Controllers;

/// <summary>
/// Internal endpoints called ONLY by the AI service.
/// Authenticated with the shared service token (see ServiceTokenAuthHandler).
/// Never exposed to React/Flutter.
/// </summary>
[ApiController]
[Route("api/internal/tools")]
[Authorize(AuthenticationSchemes = "ServiceToken")]
public class InternalToolsController : ControllerBase
{
    private readonly TravelWiseDbContext _db;

    public InternalToolsController(TravelWiseDbContext db)
    {
        _db = db;
    }

    // ---------------- Trip context ----------------

    [HttpGet("trip-context/{tripId:guid}")]
    public async Task<IActionResult> GetTripContext(Guid tripId, CancellationToken ct)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId, ct);
        if (trip is null) return NotFound(new { error = "Trip not found" });

        return Ok(new
        {
            tripId = trip.Id,
            userId = trip.UserId,
            title = trip.Title,
            destination = trip.Destination,
            startDate = trip.StartDate.ToString("yyyy-MM-dd"),
            endDate = trip.EndDate.ToString("yyyy-MM-dd"),
            currency = trip.Currency,
            tripType = trip.TripType.ToString(),
            status = trip.Status.ToString(),
            travellerCount = trip.TravellerCount
        });
    }

    // ---------------- Budget tools ----------------

    [HttpGet("budget/{tripId:guid}")]
    public async Task<IActionResult> GetTripBudget(Guid tripId, CancellationToken ct)
    {
        var budget = await _db.Budgets
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.TripId == tripId, ct);

        if (budget is null) return NotFound(new { error = "No budget set" });

        return Ok(new
        {
            budgetId = budget.Id,
            tripId = budget.TripId,
            totalAmount = budget.TotalAmount,
            currency = budget.Currency,
            categories = budget.Categories.Select(c => new
            {
                categoryId = c.Id,
                name = c.Name,
                limitAmount = c.LimitAmount
            })
        });
    }

    [HttpGet("budget/{tripId:guid}/expenses")]
    public async Task<IActionResult> GetExpenses(Guid tripId, CancellationToken ct)
    {
        var expenses = await _db.Expenses
            .Where(e => e.TripId == tripId)
            .Select(e => new
            {
                expenseId = e.Id,
                amount = e.Amount,
                currency = e.Currency,
                description = e.Description,
                spentAt = e.SpentAt,
                budgetCategoryId = e.BudgetCategoryId
            })
            .ToListAsync(ct);

        return Ok(new { tripId, expenses });
    }

    [HttpGet("budget/{tripId:guid}/health")]
    public async Task<IActionResult> CalculateBudgetHealth(Guid tripId, CancellationToken ct)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId, ct);
        if (trip is null) return NotFound(new { error = "Trip not found" });

        var budget = await _db.Budgets
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.TripId == tripId, ct);

        var expenses = await _db.Expenses.Where(e => e.TripId == tripId).ToListAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var health = TravelWise.Application.Features.Budget.BudgetHealthCalculator.Calculate(
            trip, budget, expenses, today);

        return Ok(health);
    }

    // ---------------- Activity tools ----------------

    [HttpGet("activities/search")]
    public async Task<IActionResult> SearchActivities(
        [FromQuery] string? category,
        [FromQuery] string? location,
        [FromQuery] bool? isIndoor,
        [FromQuery] decimal? maxCost,
        CancellationToken ct)
    {
        var q = _db.Activities.Where(a => a.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(location))
            q = q.Where(a => EF.Functions.ILike(a.Location, $"%{location}%"));

        if (isIndoor.HasValue)
            q = q.Where(a => a.IsIndoor == isIndoor);

        if (maxCost.HasValue)
            q = q.Where(a => a.EstimatedCost <= maxCost);

        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<ActivityCategory>(category, true, out var cat))
            q = q.Where(a => a.Category == cat);

        var list = await q
            .Select(a => new
            {
                activityId = a.Id,
                name = a.Name,
                category = a.Category.ToString(),
                description = a.Description,
                durationMinutes = a.DurationMinutes,
                estimatedCost = a.EstimatedCost,
                currency = a.Currency,
                location = a.Location,
                isIndoor = a.IsIndoor,
                difficulty = a.Difficulty
            })
            .Take(50)
            .ToListAsync(ct);

        return Ok(new { activities = list });
    }

    [HttpGet("activities/{activityId:guid}")]
    public async Task<IActionResult> GetActivityDetails(Guid activityId, CancellationToken ct)
    {
        var a = await _db.Activities.FirstOrDefaultAsync(x => x.Id == activityId, ct);
        if (a is null) return NotFound(new { error = "Activity not found" });

        return Ok(new
        {
            activityId = a.Id,
            name = a.Name,
            category = a.Category.ToString(),
            description = a.Description,
            durationMinutes = a.DurationMinutes,
            estimatedCost = a.EstimatedCost,
            currency = a.Currency,
            location = a.Location,
            isIndoor = a.IsIndoor,
            difficulty = a.Difficulty
        });
    }

    [HttpGet("activities/trip/{tripId:guid}/scheduled")]
    public async Task<IActionResult> GetScheduledActivities(Guid tripId, CancellationToken ct)
    {
        var items = await _db.ScheduledActivities
            .Include(s => s.Activity)
            .Where(s => s.TripId == tripId)
            .Select(s => new
            {
                scheduledActivityId = s.Id,
                activityId = s.ActivityId,
                activityName = s.Activity!.Name,
                category = s.Activity.Category.ToString(),
                isIndoor = s.Activity.IsIndoor,
                difficulty = s.Activity.Difficulty,
                scheduledDate = s.ScheduledDate.ToString("yyyy-MM-dd"),
                startTime = s.StartTime.ToString("HH:mm:ss"),
                endTime = s.EndTime.ToString("HH:mm:ss"),
                status = s.Status
            })
            .ToListAsync(ct);

        return Ok(new { tripId, scheduledActivities = items });
    }

    // ---------------- Risk tools ----------------

    [HttpGet("risk/{tripId:guid}/latest")]
    public async Task<IActionResult> GetLatestRiskAssessment(Guid tripId, CancellationToken ct)
    {
        var latest = await _db.RiskAssessments
            .Where(r => r.TripId == tripId)
            .OrderByDescending(r => r.AssessedAt)
            .FirstOrDefaultAsync(ct);

        if (latest is null) return NoContent();

        return Ok(new
        {
            assessmentId = latest.Id,
            tripId = latest.TripId,
            overallLevel = latest.OverallLevel.ToString(),
            detailsJson = latest.DetailsJson,
            assessedAt = latest.AssessedAt
        });
    }

    [HttpGet("risk/destination/{destination}/alerts")]
    public async Task<IActionResult> GetSafetyAlerts(string destination, CancellationToken ct)
    {
        var alerts = await _db.SafetyAlerts
            .Where(a => a.Destination == destination && a.IsActive)
            .Select(a => new
            {
                alertId = a.Id,
                destination = a.Destination,
                category = a.Category,
                severity = a.Severity.ToString(),
                message = a.Message,
                validFrom = a.ValidFrom,
                validTo = a.ValidTo
            })
            .ToListAsync(ct);

        return Ok(new { destination, alerts });
    }

    // FIXED: removed async/await — this method has no I/O.
    [HttpGet("risk/destination/{destination}/risk")]
    public Task<IActionResult> GetDestinationRisk(string destination)
    {
        var d = destination.Trim().ToLowerInvariant();

        var baseLevel = d switch
        {
            "colombo" => "Medium",
            _ => "Low"
        };

        string[] knownRisks = d switch
        {
            "ella" => new[] { "Occasional landslides during monsoon" },
            "colombo" => new[] { "Traffic congestion", "Air quality variations" },
            _ => Array.Empty<string>()
        };

        IActionResult result = Ok(new
        {
            destination,
            baseLevel,
            knownRisks
        });

        return Task.FromResult(result);
    }

    // ---------------- Document / readiness tools ----------------

    [HttpGet("readiness/{tripId:guid}/latest")]
    public async Task<IActionResult> GetLatestReadiness(Guid tripId, CancellationToken ct)
    {
        var latest = await _db.ReadinessAssessments
            .Where(r => r.TripId == tripId)
            .OrderByDescending(r => r.AssessedAt)
            .FirstOrDefaultAsync(ct);

        if (latest is null) return NoContent();

        return Ok(new
        {
            assessmentId = latest.Id,
            tripId = latest.TripId,
            score = latest.Score,
            missingJson = latest.MissingJson,
            expiringJson = latest.ExpiringJson,
            blockingJson = latest.BlockingJson,
            assessedAt = latest.AssessedAt
        });
    }

    [HttpGet("readiness/user/{userId:guid}/documents")]
    public async Task<IActionResult> GetTravellerDocuments(Guid userId, CancellationToken ct)
    {
        var docs = await _db.Documents
            .Where(d => d.UserId == userId)
            .Select(d => new
            {
                documentId = d.Id,
                type = d.Type.ToString(),
                number = d.Number,
                issuedDate = d.IssuedDate != null ? d.IssuedDate.Value.ToString("yyyy-MM-dd") : null,
                expiryDate = d.ExpiryDate != null ? d.ExpiryDate.Value.ToString("yyyy-MM-dd") : null
            })
            .ToListAsync(ct);

        return Ok(new { userId, documents = docs });
    }

    [HttpGet("readiness/destination/{destination}/requirements")]
    public async Task<IActionResult> GetRequiredDocuments(string destination, CancellationToken ct)
    {
        var reqs = await _db.DocumentRequirements
            .Where(r => r.Destination == destination)
            .Select(r => new
            {
                requirementId = r.Id,
                destination = r.Destination,
                tripType = r.TripType.ToString(),
                documentType = r.DocumentType.ToString(),
                isMandatory = r.IsMandatory,
                notes = r.Notes
            })
            .ToListAsync(ct);

        return Ok(new { destination, requirements = reqs });
    }
}