using TravelWise.Application.Features.Risk.Dtos;
using TravelWise.Domain.Enums;
using ScheduledActivityEntity = TravelWise.Domain.Entities.ScheduledActivity;
using SafetyAlertEntity = TravelWise.Domain.Entities.SafetyAlert;
using TripEntity = TravelWise.Domain.Entities.Trip;

namespace TravelWise.Application.Features.Risk;

/// <summary>
/// Deterministic trip risk assessment.
/// Combines: destination base risk + weather severity + active alerts + activity exposure.
/// Same inputs -> same output. No AI.
/// </summary>
public static class RiskCalculator
{
    // Weather thresholds (project-defined rules)
    private const double HeavyRainMmPerDay = 30.0;
    private const double StrongWindKph = 40.0;
    private const double ExtremeHeatC = 35.0;

    public static RiskAssessmentDto Calculate(
        TripEntity trip,
        IReadOnlyCollection<ScheduledActivityEntity> activities,
        IReadOnlyCollection<SafetyAlertEntity> alerts,
        WeatherDto? weatherForFirstDay)
    {
        var factors = new List<RiskFactorDto>();
        var impactedActivityIds = new List<Guid>();
        var recommendations = new List<string>();

        var maxLevel = RiskLevel.Low;

        // ---- Destination base risk (from seeded catalogue; project-defined) ----
        var destinationRisk = GetDestinationBaseRisk(trip.Destination);
        if (destinationRisk > maxLevel) maxLevel = destinationRisk;

        if (destinationRisk >= RiskLevel.Medium)
        {
            factors.Add(new RiskFactorDto
            {
                Type = "DESTINATION",
                Severity = destinationRisk,
                Description = $"Destination '{trip.Destination}' has a base risk profile of {destinationRisk}."
            });
        }

        // ---- Weather risk for the first day of the trip ----
        if (weatherForFirstDay is not null)
        {
            var weatherLevel = DeriveWeatherRisk(weatherForFirstDay);
            if (weatherLevel > RiskLevel.Low)
            {
                factors.Add(new RiskFactorDto
                {
                    Type = "WEATHER",
                    Severity = weatherLevel,
                    Description = DescribeWeatherRisk(weatherForFirstDay)
                });
                if (weatherLevel > maxLevel) maxLevel = weatherLevel;

                // Outdoor activities on the affected day are impacted
                var outdoorOnThatDay = activities
                    .Where(a => a.ScheduledDate == weatherForFirstDay.Date &&
                                a.Activity is not null &&
                                !a.Activity.IsIndoor &&
                                a.Status != "Cancelled")
                    .ToList();

                foreach (var a in outdoorOnThatDay)
                {
                    impactedActivityIds.Add(a.Id);
                    recommendations.Add(
                        $"Consider rescheduling '{a.Activity!.Name}' on {a.ScheduledDate:yyyy-MM-dd} due to weather.");
                }
            }
        }

        // ---- Active safety alerts within the trip window ----
        foreach (var alert in alerts.Where(a => a.IsActive))
        {
            // Alerts apply if their window overlaps the trip
            var alertFrom = DateOnly.FromDateTime(alert.ValidFrom);
            var alertTo = DateOnly.FromDateTime(alert.ValidTo);
            var overlapsTrip = alertFrom <= trip.EndDate && alertTo >= trip.StartDate;
            if (!overlapsTrip) continue;

            factors.Add(new RiskFactorDto
            {
                Type = "ALERT",
                Severity = alert.Severity,
                Description = $"[{alert.Category}] {alert.Message}"
            });

            if (alert.Severity > maxLevel) maxLevel = alert.Severity;

            // High/Critical alerts impact all outdoor activities in the trip window
            if (alert.Severity >= RiskLevel.High)
            {
                var affected = activities
                    .Where(a => a.Activity is not null &&
                                !a.Activity.IsIndoor &&
                                a.Status != "Cancelled" &&
                                a.ScheduledDate >= alertFrom &&
                                a.ScheduledDate <= alertTo)
                    .ToList();

                foreach (var a in affected)
                {
                    if (!impactedActivityIds.Contains(a.Id))
                        impactedActivityIds.Add(a.Id);
                }
            }
        }

        // ---- Activity-level risk (indoor vs outdoor, difficulty) ----
        var hardOutdoor = activities
            .Where(a => a.Activity is not null &&
                        !a.Activity.IsIndoor &&
                        a.Activity.Difficulty == "Hard" &&
                        a.Status != "Cancelled")
            .ToList();

        if (hardOutdoor.Count > 0 && maxLevel < RiskLevel.Medium)
        {
            factors.Add(new RiskFactorDto
            {
                Type = "ACTIVITY",
                Severity = RiskLevel.Medium,
                Description = $"{hardOutdoor.Count} hard outdoor activity(ies) planned."
            });
            maxLevel = RiskLevel.Medium;
        }

        // ---- Overall recommendations ----
        if (maxLevel >= RiskLevel.High)
            recommendations.Add("Consider revising the itinerary or adding buffer days.");
        if (maxLevel >= RiskLevel.Critical)
            recommendations.Add("Critical risk detected. Recommend contacting local authorities and considering cancellation.");

        return new RiskAssessmentDto
        {
            Id = Guid.NewGuid(),
            TripId = trip.Id,
            OverallLevel = maxLevel,
            OverallLevelName = maxLevel.ToString(),
            AssessedAt = DateTime.UtcNow,
            Factors = factors,
            ImpactedActivityIds = impactedActivityIds.Distinct().ToList(),
            Recommendations = recommendations.Distinct().ToList()
        };
    }

    // ---- Deterministic helpers ----

    public static RiskLevel DeriveWeatherRisk(WeatherDto w)
    {
        if (w.PrecipitationMm.GetValueOrDefault() >= HeavyRainMmPerDay * 2) return RiskLevel.Critical;
        if (w.PrecipitationMm.GetValueOrDefault() >= HeavyRainMmPerDay) return RiskLevel.High;
        if (w.WindSpeedKph.GetValueOrDefault() >= StrongWindKph) return RiskLevel.High;
        if (w.MaxTemperatureC.GetValueOrDefault() >= ExtremeHeatC) return RiskLevel.High;
        if (w.PrecipitationMm.GetValueOrDefault() >= 10.0) return RiskLevel.Medium;
        return RiskLevel.Low;
    }

    private static string DescribeWeatherRisk(WeatherDto w)
    {
        var parts = new List<string>();
        if (w.PrecipitationMm.HasValue) parts.Add($"{w.PrecipitationMm.Value:N1} mm rain");
        if (w.WindSpeedKph.HasValue) parts.Add($"{w.WindSpeedKph.Value:N1} kph wind");
        if (w.MaxTemperatureC.HasValue) parts.Add($"max {w.MaxTemperatureC.Value:N1}°C");
        return parts.Count > 0 ? string.Join(", ", parts) : "Unfavourable weather conditions.";
    }

    /// <summary>
    /// Very small project-defined lookup. Real destination risk would come from a
    /// proper source; these values are illustrative only.
    /// </summary>
    private static RiskLevel GetDestinationBaseRisk(string destination)
    {
        return destination.Trim().ToLowerInvariant() switch
        {
            "ella" => RiskLevel.Low,
            "colombo" => RiskLevel.Medium,
            "kandy" => RiskLevel.Low,
            "galle" => RiskLevel.Low,
            _ => RiskLevel.Low
        };
    }
}