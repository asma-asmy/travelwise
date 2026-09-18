using TravelWise.Domain.Common;

namespace TravelWise.Domain.Entities;

public class Expense : BaseEntity
{
    public Guid TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    public Guid? BudgetCategoryId { get; set; }
    public BudgetCategory? BudgetCategory { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "LKR";
    public string Description { get; set; } = string.Empty;
    public DateTime SpentAt { get; set; } = DateTime.UtcNow;
}