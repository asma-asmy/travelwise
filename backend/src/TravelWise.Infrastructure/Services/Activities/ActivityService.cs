using Microsoft.EntityFrameworkCore;
using TravelWise.Application.Common.Exceptions;
using TravelWise.Application.Features.Activities;
using TravelWise.Application.Features.Activities.Dtos;
using TravelWise.Application.Features.Budget.Dtos;    // PagedResult<T>
using TravelWise.Domain.Entities;
using TravelWise.Infrastructure.Persistence;
using ActivityEntity = TravelWise.Domain.Entities.Activity;
using ScheduledActivityEntity = TravelWise.Domain.Entities.ScheduledActivity;
using TripEntity = TravelWise.Domain.Entities.Trip;
using TravellerPreferenceEntity = TravelWise.Domain.Entities.TravellerPreference;

namespace TravelWise.Infrastructure.Services.Activities;

public class ActivityService : IActivityService
{
    private readonly TravelWiseDbContext _db;

    public ActivityService(TravelWiseDbContext db)
    {
        _db = db;
    }

    // ---------------- Catalogue ----------------

    public async Task<PagedResult<ActivityDto>> SearchAsync(ActivityQueryDto query, CancellationToken ct = default)
    {
        var q = _db.Activities.Where(a => a.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(a => EF.Functions.ILike(a.Name, $"%{query.Search}%")
                          || EF.Functions.ILike(a.Description, $"%{query.Search}%"));

        if (query.Category.HasValue)
            q = q.Where(a => a.Category == query.Category);

        if (!string.IsNullOrWhiteSpace(query.Location))
            q = q.Where(a => EF.Functions.ILike(a.Location, $"%{query.Location}%"));

        if (query.IsIndoor.HasValue)
            q = q.Where(a => a.IsIndoor == query.IsIndoor);

        if (!string.IsNullOrWhiteSpace(query.Difficulty))
            q = q.Where(a => a.Difficulty == query.Difficulty);

        if (query.MinCost.HasValue)
            q = q.Where(a => a.EstimatedCost >= query.MinCost);

        if (query.MaxCost.HasValue)
            q = q.Where(a => a.EstimatedCost <= query.MaxCost);

        if (query.MaxDurationMinutes.HasValue)
            q = q.Where(a => a.DurationMinutes <= query.MaxDurationMinutes);

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "cost"     => query.SortDir == "desc" ? q.OrderByDescending(a => a.EstimatedCost) : q.OrderBy(a => a.EstimatedCost),
            "duration" => query.SortDir == "desc" ? q.OrderByDescending(a => a.DurationMinutes) : q.OrderBy(a => a.DurationMinutes),
            _          => query.SortDir == "desc" ? q.OrderByDescending(a => a.Name) : q.OrderBy(a => a.Name)
        };

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => MapActivity(a)).ToListAsync(ct);

        return new PagedResult<ActivityDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ActivityDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var activity = await _db.Activities.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException("Activity not found.");
        return MapActivity(activity);
    }

    public async Task<ActivityDto> CreateAsync(CreateActivityDto dto, CancellationToken ct = default)
    {
        var activity = new ActivityEntity
        {
            Name = dto.Name.Trim(),
            Category = dto.Category,
            Description = dto.Description.Trim(),
            DurationMinutes = dto.DurationMinutes,
            EstimatedCost = dto.EstimatedCost,
            Currency = dto.Currency,
            Location = dto.Location.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsIndoor = dto.IsIndoor,
            Difficulty = dto.Difficulty,
            IsActive = true
        };
        _db.Activities.Add(activity);
        await _db.SaveChangesAsync(ct);
        return MapActivity(activity);
    }

    public async Task<ActivityDto> UpdateAsync(Guid id, UpdateActivityDto dto, CancellationToken ct = default)
    {
        var activity = await _db.Activities.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException("Activity not found.");

        activity.Name = dto.Name.Trim();
        activity.Category = dto.Category;
        activity.Description = dto.Description.Trim();
        activity.DurationMinutes = dto.DurationMinutes;
        activity.EstimatedCost = dto.EstimatedCost;
        activity.Currency = dto.Currency;
        activity.Location = dto.Location.Trim();
        activity.Latitude = dto.Latitude;
        activity.Longitude = dto.Longitude;
        activity.IsIndoor = dto.IsIndoor;
        activity.Difficulty = dto.Difficulty;
        activity.IsActive = dto.IsActive;

        await _db.SaveChangesAsync(ct);
        return MapActivity(activity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var activity = await _db.Activities.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException("Activity not found.");

        // Soft delete: deactivate, so existing schedule references remain intact.
        activity.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    // ---------------- Preferences ----------------

    public async Task<TravellerPreferenceDto?> GetMyPreferencesAsync(Guid userId, CancellationToken ct = default)
    {
        var pref = await _db.TravellerPreferences.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        return pref is null ? null : MapPreference(pref);
    }

    public async Task<TravellerPreferenceDto> UpsertMyPreferencesAsync(Guid userId, UpsertTravellerPreferenceDto dto, CancellationToken ct = default)
    {
        var pref = await _db.TravellerPreferences.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (pref is null)
        {
            pref = new TravellerPreferenceEntity { UserId = userId };
            _db.TravellerPreferences.Add(pref);
        }

        pref.InterestsJson = System.Text.Json.JsonSerializer.Serialize(dto.Interests);
        pref.Pace = dto.Pace;
        pref.MobilityNotes = dto.MobilityNotes;
        pref.DietaryJson = System.Text.Json.JsonSerializer.Serialize(dto.Dietary);
        pref.BudgetTier = dto.BudgetTier;

        await _db.SaveChangesAsync(ct);
        return MapPreference(pref);
    }

    // ---------------- Scheduling ----------------

    public async Task<List<ScheduledActivityDto>> ListScheduledAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default)
    {
        await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var items = await _db.ScheduledActivities
            .Include(s => s.Activity)
            .Where(s => s.TripId == tripId)
            .OrderBy(s => s.ScheduledDate).ThenBy(s => s.StartTime)
            .ToListAsync(ct);

        return items.Select(MapScheduled).ToList();
    }

    public async Task<ScheduledActivityDto> ScheduleAsync(Guid tripId, Guid userId, bool isStaff, CreateScheduledActivityDto dto, CancellationToken ct = default)
    {
        var trip = await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var activity = await _db.Activities.FirstOrDefaultAsync(a => a.Id == dto.ActivityId && a.IsActive, ct)
            ?? throw new NotFoundException("Activity not found or inactive.");

        // Business rule: scheduled date must be within trip range
        if (dto.ScheduledDate < trip.StartDate || dto.ScheduledDate > trip.EndDate)
            throw new AppException($"Scheduled date must be between {trip.StartDate} and {trip.EndDate}.", 422);

        var endTime = dto.StartTime.AddMinutes(activity.DurationMinutes);

        // Business rule: time conflict check
        var sameDay = await _db.ScheduledActivities
            .Include(s => s.Activity)
            .Where(s => s.TripId == tripId && s.ScheduledDate == dto.ScheduledDate && s.Status != "Cancelled")
            .ToListAsync(ct);

        foreach (var s in sameDay)
        {
            if (dto.StartTime < s.EndTime && s.StartTime < endTime)
                throw new AppException(
                    $"Time conflict with '{s.Activity?.Name}' scheduled from {s.StartTime} to {s.EndTime}.",
                    422);
        }

        var scheduled = new ScheduledActivityEntity
        {
            TripId = tripId,
            ActivityId = dto.ActivityId,
            ScheduledDate = dto.ScheduledDate,
            StartTime = dto.StartTime,
            EndTime = endTime,
            Status = "Planned",
            Notes = dto.Notes
        };

        _db.ScheduledActivities.Add(scheduled);
        await _db.SaveChangesAsync(ct);

        await _db.Entry(scheduled).Reference(s => s.Activity).LoadAsync(ct);
        return MapScheduled(scheduled);
    }

    public async Task<ScheduledActivityDto> UpdateScheduledAsync(Guid tripId, Guid scheduledId, Guid userId, bool isStaff, UpdateScheduledActivityDto dto, CancellationToken ct = default)
    {
        var trip = await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var scheduled = await _db.ScheduledActivities
            .Include(s => s.Activity)
            .FirstOrDefaultAsync(s => s.Id == scheduledId && s.TripId == tripId, ct)
            ?? throw new NotFoundException("Scheduled activity not found.");

        if (dto.ScheduledDate < trip.StartDate || dto.ScheduledDate > trip.EndDate)
            throw new AppException($"Scheduled date must be between {trip.StartDate} and {trip.EndDate}.", 422);

        var endTime = dto.StartTime.AddMinutes(scheduled.Activity?.DurationMinutes ?? 0);

        // Conflict check excluding this one
        var sameDay = await _db.ScheduledActivities
            .Include(s => s.Activity)
            .Where(s => s.TripId == tripId && s.ScheduledDate == dto.ScheduledDate && s.Status != "Cancelled" && s.Id != scheduledId)
            .ToListAsync(ct);

        foreach (var s in sameDay)
        {
            if (dto.StartTime < s.EndTime && s.StartTime < endTime)
                throw new AppException(
                    $"Time conflict with '{s.Activity?.Name}' scheduled from {s.StartTime} to {s.EndTime}.",
                    422);
        }

        scheduled.ScheduledDate = dto.ScheduledDate;
        scheduled.StartTime = dto.StartTime;
        scheduled.EndTime = endTime;
        scheduled.Status = dto.Status;
        scheduled.Notes = dto.Notes;

        await _db.SaveChangesAsync(ct);
        return MapScheduled(scheduled);
    }

    public async Task DeleteScheduledAsync(Guid tripId, Guid scheduledId, Guid userId, bool isStaff, CancellationToken ct = default)
    {
        await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var scheduled = await _db.ScheduledActivities.FirstOrDefaultAsync(s => s.Id == scheduledId && s.TripId == tripId, ct)
            ?? throw new NotFoundException("Scheduled activity not found.");

        _db.ScheduledActivities.Remove(scheduled);
        await _db.SaveChangesAsync(ct);
    }

    // ---------------- Business Operations ----------------

    public async Task<List<ScheduleConflictDto>> GetConflictsAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default)
    {
        await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var scheduled = await _db.ScheduledActivities
            .Include(s => s.Activity)
            .Where(s => s.TripId == tripId && s.Status != "Cancelled")
            .ToListAsync(ct);

        return ScheduleConflictDetector.Detect(scheduled);
    }

    public async Task<ScheduleOptimizationDto> OptimizeAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default)
    {
        var trip = await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var scheduled = await _db.ScheduledActivities
            .Include(s => s.Activity)
            .Where(s => s.TripId == tripId)
            .ToListAsync(ct);

        var catalogue = await _db.Activities.Where(a => a.IsActive).ToListAsync(ct);

        var preference = await _db.TravellerPreferences.FirstOrDefaultAsync(p => p.UserId == trip.UserId, ct);

        return ScheduleOptimizer.Analyze(trip, scheduled, catalogue, preference);
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

    private static ActivityDto MapActivity(ActivityEntity a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        Category = a.Category,
        CategoryName = a.Category.ToString(),
        Description = a.Description,
        DurationMinutes = a.DurationMinutes,
        EstimatedCost = a.EstimatedCost,
        Currency = a.Currency,
        Location = a.Location,
        Latitude = a.Latitude,
        Longitude = a.Longitude,
        IsIndoor = a.IsIndoor,
        Difficulty = a.Difficulty,
        IsActive = a.IsActive,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };

    private static ScheduledActivityDto MapScheduled(ScheduledActivityEntity s) => new()
    {
        Id = s.Id,
        TripId = s.TripId,
        ActivityId = s.ActivityId,
        ActivityName = s.Activity?.Name ?? string.Empty,
        ActivityCategory = s.Activity?.Category.ToString() ?? string.Empty,
        Location = s.Activity?.Location ?? string.Empty,
        IsIndoor = s.Activity?.IsIndoor ?? false,
        DurationMinutes = s.Activity?.DurationMinutes ?? 0,
        EstimatedCost = s.Activity?.EstimatedCost ?? 0m,
        ScheduledDate = s.ScheduledDate,
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        Status = s.Status,
        Notes = s.Notes
    };

    private static TravellerPreferenceDto MapPreference(TravellerPreferenceEntity p)
    {
        List<string> interests;
        List<string> dietary;
        try { interests = System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.InterestsJson) ?? new(); }
        catch { interests = new(); }
        try { dietary = System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.DietaryJson) ?? new(); }
        catch { dietary = new(); }

        return new TravellerPreferenceDto
        {
            UserId = p.UserId,
            Interests = interests,
            Pace = p.Pace,
            MobilityNotes = p.MobilityNotes,
            Dietary = dietary,
            BudgetTier = p.BudgetTier
        };
    }
}