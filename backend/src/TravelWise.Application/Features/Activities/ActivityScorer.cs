using TravelWise.Application.Features.Activities.Dtos;
using TravelWise.Domain.Entities;
using ActivityEntity = TravelWise.Domain.Entities.Activity;

namespace TravelWise.Application.Features.Activities;

/// <summary>
/// Deterministic preference fit scorer. 0..100.
/// Higher score = better fit for the traveller.
/// </summary>
public static class ActivityScorer
{
    public static double Score(ActivityEntity activity, TravellerPreference? preference)
    {
        if (preference is null) return 50.0;

        double score = 0.0;

        // Interest match (max 40)
        var interests = ParseInterests(preference.InterestsJson);
        if (interests.Count > 0)
        {
            var category = activity.Category.ToString().ToLowerInvariant();
            var matched = interests.Count(i => category.Contains(i.ToLowerInvariant()));
            score += Math.Min(40.0, 40.0 * matched / interests.Count);
        }
        else
        {
            score += 20.0;
        }

        // Budget tier match (max 25)
        score += preference.BudgetTier switch
        {
            "Budget" => activity.EstimatedCost <= 1000 ? 25.0 : activity.EstimatedCost <= 3000 ? 15.0 : 0.0,
            "Mid"    => activity.EstimatedCost <= 5000 ? 25.0 : activity.EstimatedCost <= 10000 ? 15.0 : 5.0,
            "Luxury" => 25.0,
            _        => 15.0
        };

        // Pace match (max 15)
        score += preference.Pace switch
        {
            "Relaxed"  => activity.DurationMinutes <= 90 ? 15.0 : activity.DurationMinutes <= 150 ? 8.0 : 0.0,
            "Moderate" => activity.DurationMinutes <= 180 ? 15.0 : 8.0,
            "Packed"   => 15.0,
            _          => 8.0
        };

        // Difficulty vs mobility (max 10)
        if (!string.IsNullOrWhiteSpace(preference.MobilityNotes) &&
            activity.Difficulty.Equals("Hard", StringComparison.OrdinalIgnoreCase))
            score += 0.0;
        else
            score += 10.0;

        // Baseline: new/valuable activities get slight bonus (max 10)
        score += 10.0;

        return Math.Round(Math.Clamp(score, 0.0, 100.0), 2);
    }

    private static List<string> ParseInterests(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}