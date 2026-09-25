using TravelWise.Application.Features.Budget.Dtos;
using TravelWise.Domain.Entities;

namespace TravelWise.Application.Features.Budget;

/// <summary>
/// Deterministic budget health calculator.
/// No AI. No randomness. Same inputs -> same outputs.
/// Business rules enforced here are the ones the Budget Agent depends on.
/// </summary>
public static class BudgetHealthCalculator
{
    // Business thresholds (project-defined rules)
    private const decimal WatchThresholdPercent = 60m;      // 60% spent
    private const decimal AtRiskThresholdPercent = 85m;     // 85% spent
    private const decimal OverrunThresholdPercent = 100m;   // 100% spent

    public static BudgetHealthDto Calculate(
        Trip trip,
        TravelWise.Domain.Entities.Budget? budget,
        IReadOnlyCollection<Expense> expenses,
        DateOnly today)
    {
        var totalBudget = budget?.TotalAmount ?? 0m;
        var totalSpent = expenses.Sum(e => e.Amount);
        var remaining = totalBudget - totalSpent;

        var totalDays = Math.Max(1, trip.EndDate.DayNumber - trip.StartDate.DayNumber + 1);
        var elapsedDays = Math.Clamp(today.DayNumber - trip.StartDate.DayNumber + 1, 0, totalDays);
        var remainingDays = Math.Max(0, totalDays - elapsedDays);

        var avgDailySpend = elapsedDays > 0 ? totalSpent / elapsedDays : 0m;
        var recommendedDailyCap = remainingDays > 0 ? Math.Max(0m, remaining / remainingDays) : 0m;
        var projectedEndBalance = remaining - (avgDailySpend * remainingDays);

        var utilizationPercent = totalBudget > 0 ? (totalSpent / totalBudget) * 100m : 0m;

        var status =
            totalBudget <= 0 ? "AT_RISK" :
            utilizationPercent >= OverrunThresholdPercent ? "OVER" :
            utilizationPercent >= AtRiskThresholdPercent ? "AT_RISK" :
            utilizationPercent >= WatchThresholdPercent ? "WATCH" :
            "HEALTHY";

        var warnings = new List<string>();
        if (totalBudget <= 0) warnings.Add("No budget has been set for this trip.");
        if (remaining < 0) warnings.Add($"You have overspent by {Math.Abs(remaining):N2} {budget?.Currency ?? "LKR"}.");
        if (projectedEndBalance < 0 && remaining >= 0)
            warnings.Add($"At the current spend rate, you are projected to be short by {Math.Abs(projectedEndBalance):N2} at the end of the trip.");
        if (remainingDays > 0 && remaining > 0 && avgDailySpend > recommendedDailyCap)
            warnings.Add($"Your daily average spend exceeds the recommended cap of {recommendedDailyCap:N2}.");

        // Per-category overruns
        var overruns = new List<CategoryOverrunDto>();
        if (budget is not null)
        {
            foreach (var category in budget.Categories)
            {
                var spent = expenses.Where(e => e.BudgetCategoryId == category.Id).Sum(e => e.Amount);
                var utilization = category.LimitAmount > 0 ? (spent / category.LimitAmount) * 100m : 0m;

                if (spent > category.LimitAmount)
                {
                    overruns.Add(new CategoryOverrunDto
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name,
                        LimitAmount = category.LimitAmount,
                        SpentAmount = spent,
                        OverrunAmount = spent - category.LimitAmount,
                        UtilizationPercent = Math.Round(utilization, 2)
                    });
                }
            }
        }

        return new BudgetHealthDto
        {
            TripId = trip.Id,
            HealthStatus = status,
            TotalBudget = totalBudget,
            TotalSpent = totalSpent,
            RemainingAmount = remaining,
            TripDaysTotal = totalDays,
            TripDaysElapsed = elapsedDays,
            TripDaysRemaining = remainingDays,
            AverageDailySpend = Math.Round(avgDailySpend, 2),
            RecommendedDailyCap = Math.Round(recommendedDailyCap, 2),
            ProjectedEndBalance = Math.Round(projectedEndBalance, 2),
            CategoryOverruns = overruns,
            Warnings = warnings
        };
    }
}