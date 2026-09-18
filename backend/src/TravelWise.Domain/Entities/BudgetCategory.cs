using TravelWise.Domain.Common;

namespace TravelWise.Domain.Entities;

public class BudgetCategory : BaseEntity
{
    public Guid BudgetId { get; set; }
    public Budget Budget { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public decimal LimitAmount { get; set; }

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}