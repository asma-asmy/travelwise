namespace TravelWise.Application.Features.Budget.Dtos;

public class ExpenseDto
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public Guid? BudgetCategoryId { get; set; }
    public string? BudgetCategoryName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime SpentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}