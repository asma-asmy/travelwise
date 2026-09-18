using TravelWise.Domain.Common;

namespace TravelWise.Domain.Entities;

public class Budget : BaseEntity
{
    public Guid TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "LKR";

    public ICollection<BudgetCategory> Categories { get; set; } = new List<BudgetCategory>();
}