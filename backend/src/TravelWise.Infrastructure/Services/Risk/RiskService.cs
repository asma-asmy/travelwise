using Microsoft.EntityFrameworkCore;
using TravelWise.Application.Common.Exceptions;
using TravelWise.Application.Common.Interfaces;
using TravelWise.Application.Features.Budget.Dtos;
using TravelWise.Application.Features.Risk;
using TravelWise.Application.Features.Risk.Dtos;
using TravelWise.Domain.Entities;
using TravelWise.Infrastructure.Persistence;
using RiskAssessmentEntity = TravelWise.Domain.Entities.RiskAssessment;
using SafetyAlertEntity = TravelWise.Domain.Entities.SafetyAlert;
using TripEntity = TravelWise.Domain.Entities.Trip;

namespace TravelWise.Infrastructure.Services.Risk;

public class RiskService : IRiskService
{
    private readonly TravelWiseDbContext _db;
    private readonly IWeatherClient _weather;

    public RiskService(TravelWiseDbContext db, IWeatherClient weather)
    {
        _db = db;
        _weather = weather;
    }

    // ---------------- Alerts ----------------

    public async Task<PagedResult<SafetyAlertDto>> ListAlertsAsync(SafetyAlertQueryDto query, CancellationToken ct = default)
    {
        var q = _db.SafetyAlerts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Destination))
            q = q.Where(a => EF.Functions.ILike(a.Destination, $"%{query.Destination}%"));

        if (!string.IsNullOrWhiteSpace(query.Category))
            q = q.Where(a => a.Category == query.Category);

        if (query.MinSeverity.HasValue)
            q = q.Where(a => a.Severity >= query.MinSeverity);

        if (query.IsActive.HasValue)
            q = q.Where(a => a.IsActive == query.IsActive);

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "severity" => query.SortDir == "asc" ? q.OrderBy(a => a.Severity) : q.OrderByDescending(a => a.Severity),
            _ => query.SortDir == "asc" ? q.OrderBy(a => a.ValidFrom) : q.OrderByDescending(a => a.ValidFrom)
        };

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => MapAlert(a)).ToListAsync(ct);

        return new PagedResult<SafetyAlertDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<SafetyAlertDto> CreateAlertAsync(CreateSafetyAlertDto dto, CancellationToken ct = default)
    {
        var alert = new SafetyAlertEntity
        {
            Destination = dto.Destination.Trim(),
            Category = dto.Category.Trim(),
            Severity = dto.Severity,
            Message = dto.Message.Trim(),
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            IsActive = true
        };
        _db.SafetyAlerts.Add(alert);
        await _db.SaveChangesAsync(ct);
        return MapAlert(alert);
    }

    public async Task<SafetyAlertDto> UpdateAlertAsync(Guid id, UpdateSafetyAlertDto dto, CancellationToken ct = default)
    {
        var alert = await _db.SafetyAlerts.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException("Safety alert not found.");

        alert.Category = dto.Category.Trim();
        alert.Severity = dto.Severity;
        alert.Message = dto.Message.Trim();
        alert.ValidFrom = dto.ValidFrom;
        alert.ValidTo = dto.ValidTo;
        alert.IsActive = dto.IsActive;

        await _db.SaveChangesAsync(ct);
        return MapAlert(alert);
    }

    public async Task DeleteAlertAsync(Guid id, CancellationToken ct = default)
    {
        var alert = await _db.SafetyAlerts.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException("Safety alert not found.");

        // Soft delete: deactivate
        alert.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    // ---------------- Destination risk ----------------

    public Task<DestinationRiskDto> GetDestinationRiskAsync(string destination, CancellationToken ct = default)
    {
        var normalized = destination.Trim();
        var dto = new DestinationRiskDto
        {
            Destination = normalized,
            BaseLevel = GetBaseLevel(normalized),
            BaseLevelName = GetBaseLevel(normalized).ToString(),
            KnownRisks = GetKnownRisks(normalized),
            Description = GetDescription(normalized)
        };
        return Task.FromResult(dto);
    }

    // ---------------- Weather ----------------

    public async Task<WeatherDto?> GetWeatherAsync(string destination, DateOnly date, CancellationToken ct = default)
    {
        // Use Ella coordinates for demo; a real system would geocode
        var (lat, lon) = ResolveCoordinates(destination);
        return await _weather.GetWeatherAsync(destination, lat, lon, date, ct);
    }

    // ---------------- Safety info ----------------

    public async Task<SafetyInformationDto> GetSafetyInformationAsync(string destination, CancellationToken ct = default)
    {
        var alerts = await _db.SafetyAlerts
            .Where(a => a.Destination == destination && a.IsActive)
            .OrderByDescending(a => a.Severity)
            .ToListAsync(ct);

        return new SafetyInformationDto
        {
            Destination = destination,
            EmergencyNumber = "119",                            // Sri Lanka general emergency (project-defined)
            HospitalInfo = "Nearest hospital: District General Hospital",
            LocalAdvisory = "Check local news for up-to-date advisories.",
            ActiveAlerts = alerts.Select(MapAlert).ToList()
        };
    }

    // ---------------- Business operations ----------------

    public async Task<RiskAssessmentDto> AssessTripAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default)
    {
        var trip = await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var activities = await _db.ScheduledActivities
            .Include(s => s.Activity)
            .Where(s => s.TripId == tripId)
            .ToListAsync(ct);

        var alerts = await _db.SafetyAlerts
            .Where(a => a.Destination == trip.Destination && a.IsActive)
            .ToListAsync(ct);

        // Fetch weather for the first day of the trip
        var firstDay = trip.StartDate;
        var weather = await GetWeatherAsync(trip.Destination, firstDay, ct);

        var result = RiskCalculator.Calculate(trip, activities, alerts, weather);

        // Persist the assessment
        var entity = new RiskAssessmentEntity
        {
            TripId = trip.Id,
            OverallLevel = result.OverallLevel,
            DetailsJson = System.Text.Json.JsonSerializer.Serialize(result),
            AssessedAt = DateTime.UtcNow
        };
        _db.RiskAssessments.Add(entity);
        await _db.SaveChangesAsync(ct);

        result.Id = entity.Id;
        return result;
    }

    public async Task<RiskAssessmentDto?> GetLatestAssessmentAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default)
    {
        await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var latest = await _db.RiskAssessments
            .Where(r => r.TripId == tripId)
            .OrderByDescending(r => r.AssessedAt)
            .FirstOrDefaultAsync(ct);

        if (latest is null) return null;

        try
        {
            var dto = System.Text.Json.JsonSerializer.Deserialize<RiskAssessmentDto>(latest.DetailsJson);
            if (dto is not null)
            {
                dto.Id = latest.Id;
                dto.AssessedAt = latest.AssessedAt;
            }
            return dto;
        }
        catch
        {
            return null;
        }
    }

    // ---------------- Helpers ----------------

    private async Task<TripEntity> EnsureTripAccessAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId, ct)
            ?? throw new NotFoundException("Trip not found.");

        if (!isStaff && trip.UserId != userId)
            throw new ForbiddenException("You do not have access to this trip.");

        return trip;
    }

    private static SafetyAlertDto MapAlert(SafetyAlertEntity a) => new()
    {
        Id = a.Id,
        Destination = a.Destination,
        Category = a.Category,
        Severity = a.Severity,
        SeverityName = a.Severity.ToString(),
        Message = a.Message,
        ValidFrom = a.ValidFrom,
        ValidTo = a.ValidTo,
        IsActive = a.IsActive
    };

    private static TravelWise.Domain.Enums.RiskLevel GetBaseLevel(string destination) =>
        destination.ToLowerInvariant() switch
        {
            "ella" => TravelWise.Domain.Enums.RiskLevel.Low,
            "colombo" => TravelWise.Domain.Enums.RiskLevel.Medium,
            "kandy" => TravelWise.Domain.Enums.RiskLevel.Low,
            "galle" => TravelWise.Domain.Enums.RiskLevel.Low,
            _ => TravelWise.Domain.Enums.RiskLevel.Low
        };

    private static List<string> GetKnownRisks(string destination) =>
        destination.ToLowerInvariant() switch
        {
            "ella" => new List<string> { "Occasional landslides during monsoon" },
            "colombo" => new List<string> { "Traffic congestion", "Air quality variations" },
            _ => new List<string>()
        };

    private static string? GetDescription(string destination) =>
        destination.ToLowerInvariant() switch
        {
            "ella" => "Hill country destination. Best visited in dry season.",
            "colombo" => "Urban coastal city. Standard precautions apply.",
            _ => null
        };

    private static (double lat, double lon) ResolveCoordinates(string destination) =>
        destination.ToLowerInvariant() switch
        {
            "ella" => (6.8667, 81.0467),
            "colombo" => (6.9271, 79.8612),
            "kandy" => (7.2906, 80.6337),
            "galle" => (6.0535, 80.2210),
            _ => (6.8667, 81.0467)   // fallback to Ella
        };
}