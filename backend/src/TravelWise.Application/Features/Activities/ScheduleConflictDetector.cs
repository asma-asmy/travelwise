using TravelWise.Application.Features.Activities.Dtos;
using ScheduledActivityEntity = TravelWise.Domain.Entities.ScheduledActivity;

namespace TravelWise.Application.Features.Activities;

/// <summary>
/// Deterministic schedule conflict detection.
/// A conflict exists if any two scheduled activities on the same day overlap in time.
/// </summary>
public static class ScheduleConflictDetector
{
    public static List<ScheduleConflictDto> Detect(IEnumerable<ScheduledActivityEntity> scheduled)
    {
        var conflicts = new List<ScheduleConflictDto>();
        var list = scheduled.Where(s => s.Status != "Cancelled").ToList();

        // Group by date, then check pairwise overlap within each day
        foreach (var day in list.GroupBy(s => s.ScheduledDate))
        {
            var items = day.OrderBy(s => s.StartTime).ToList();
            for (int i = 0; i < items.Count; i++)
            {
                for (int j = i + 1; j < items.Count; j++)
                {
                    if (Overlaps(items[i], items[j]))
                    {
                        conflicts.Add(new ScheduleConflictDto
                        {
                            ConflictingScheduledActivityId = items[i].Id,
                            ConflictingActivityId = items[i].ActivityId,
                            ConflictingActivityName = items[i].Activity?.Name ?? "(unknown)",
                            ScheduledDate = items[i].ScheduledDate,
                            StartTime = items[i].StartTime,
                            EndTime = items[i].EndTime,
                            Reason = "TIME_OVERLAP"
                        });
                    }
                }
            }
        }

        return conflicts;
    }

    public static bool Overlaps(ScheduledActivityEntity a, ScheduledActivityEntity b)
    {
        if (a.ScheduledDate != b.ScheduledDate) return false;
        return a.StartTime < b.EndTime && b.StartTime < a.EndTime;
    }
}