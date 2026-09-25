using TravelWise.Application.Features.Activities.Dtos;
using TripEntity = TravelWise.Domain.Entities.Trip;
using ActivityEntity = TravelWise.Domain.Entities.Activity;
using ScheduledActivityEntity = TravelWise.Domain.Entities.ScheduledActivity;
using TravellerPreferenceEntity = TravelWise.Domain.Entities.TravellerPreference;

namespace TravelWise.Application.Features.Activities;

/// <summary>
/// Business operation: Smart Activity Schedule Optimization.
/// Deterministic analysis of a trip's schedule + recommendations from the catalogue.
/// </summary>
public static class ScheduleOptimizer
{
    private const int MaxDailyMinutes = 600;    // 10 hours
    private const int MaxRecommendations = 10;

    public static ScheduleOptimizationDto Analyze(
        TripEntity trip,
        IReadOnlyCollection<ScheduledActivityEntity> scheduled,
        IReadOnlyCollection<ActivityEntity> catalogue,
        TravellerPreferenceEntity? preference)
    {
        var active = scheduled.Where(s => s.Status != "Cancelled").ToList();
        var totalMinutes = active.Sum(s => s.Activity?.DurationMinutes ?? 0);
        var totalCost = active.Sum(s => s.Activity?.EstimatedCost ?? 0m);

        var conflicts = ScheduleConflictDetector.Detect(active);

        var warnings = new List<string>();
        var daysCount = Math.Max(1, trip.EndDate.DayNumber - trip.StartDate.DayNumber + 1);

        // Per-day duration warning
        foreach (var day in active.GroupBy(s => s.ScheduledDate))
        {
            var dayMinutes = day.Sum(s => s.Activity?.DurationMinutes ?? 0);
            if (dayMinutes > MaxDailyMinutes)
                warnings.Add($"On {day.Key:yyyy-MM-dd}, activities total {dayMinutes} minutes, exceeding the daily cap of {MaxDailyMinutes}.");
        }

        if (conflicts.Count > 0)
            warnings.Add($"{conflicts.Count} scheduling conflict(s) detected. Resolve overlaps before finalizing.");

        if (active.Count == 0)
            warnings.Add("No activities are scheduled yet. Consider adding some from recommendations.");

        // Recommendations from the catalogue not already scheduled
        var scheduledIds = active.Select(s => s.ActivityId).ToHashSet();
        var candidates = catalogue
            .Where(a => a.IsActive && !scheduledIds.Contains(a.Id))
            .ToList();

        var scored = candidates
            .Select(a => new ActivityRecommendationDto
            {
                ActivityId = a.Id,
                Name = a.Name,
                Category = a.Category.ToString(),
                EstimatedCost = a.EstimatedCost,
                DurationMinutes = a.DurationMinutes,
                PreferenceScore = ActivityScorer.Score(a, preference),
                IsIndoor = a.IsIndoor,
                Location = a.Location
            })
            .OrderByDescending(r => r.PreferenceScore)
            .ThenBy(r => r.EstimatedCost)
            .Take(MaxRecommendations)
            .ToList();

        return new ScheduleOptimizationDto
        {
            TripId = trip.Id,
            TotalActivitiesScheduled = active.Count,
            TotalMinutesScheduled = totalMinutes,
            TotalEstimatedCost = totalCost,
            PreferenceMatchCount = scored.Count(r => r.PreferenceScore >= 70.0),
            Conflicts = conflicts,
            Recommendations = scored,
            Warnings = warnings
        };
    }
}