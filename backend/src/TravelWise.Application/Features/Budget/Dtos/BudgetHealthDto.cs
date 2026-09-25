namespace TravelWise.Application.Features.Budget.Dtos;

public class BudgetHealthDto
{
    public Guid TripId { get; set; }
    public string HealthStatus { get; set; } = string.Empty;    // HEALTHY | WATCH | AT_RISK | OVER
    public decimal TotalBudget { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal RemainingAmount { get; set; }
    public int TripDaysTotal { get; set; }
    public int TripDaysElapsed { get; set; }
    public int TripDaysRemaining { get; set; }
    public decimal AverageDailySpend { get; set; }
    public decimal RecommendedDailyCap { get; set; }
    public decimal ProjectedEndBalance { get; set; }
    public List<CategoryOverrunDto> CategoryOverruns { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class CategoryOverrunDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal LimitAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal OverrunAmount { get; set; }
    public decimal UtilizationPercent { get; set; }
}