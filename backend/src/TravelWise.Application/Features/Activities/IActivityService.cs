using TravelWise.Application.Features.Activities.Dtos;
using TravelWise.Application.Features.Budget.Dtos;   // for PagedResult<T>

namespace TravelWise.Application.Features.Activities;

public interface IActivityService
{
    // Catalogue
    Task<PagedResult<ActivityDto>> SearchAsync(ActivityQueryDto query, CancellationToken ct = default);
    Task<ActivityDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<ActivityDto> CreateAsync(CreateActivityDto dto, CancellationToken ct = default);
    Task<ActivityDto> UpdateAsync(Guid id, UpdateActivityDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    // Preferences
    Task<TravellerPreferenceDto?> GetMyPreferencesAsync(Guid userId, CancellationToken ct = default);
    Task<TravellerPreferenceDto> UpsertMyPreferencesAsync(Guid userId, UpsertTravellerPreferenceDto dto, CancellationToken ct = default);

    // Scheduling
    Task<List<ScheduledActivityDto>> ListScheduledAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default);
    Task<ScheduledActivityDto> ScheduleAsync(Guid tripId, Guid userId, bool isStaff, CreateScheduledActivityDto dto, CancellationToken ct = default);
    Task<ScheduledActivityDto> UpdateScheduledAsync(Guid tripId, Guid scheduledId, Guid userId, bool isStaff, UpdateScheduledActivityDto dto, CancellationToken ct = default);
    Task DeleteScheduledAsync(Guid tripId, Guid scheduledId, Guid userId, bool isStaff, CancellationToken ct = default);

    // Business operations
    Task<List<ScheduleConflictDto>> GetConflictsAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default);
    Task<ScheduleOptimizationDto> OptimizeAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default);
}