namespace TravelWise.Application.Features.Budget.Dtos;

public class BudgetDto
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public decimal RemainingAmount { get; set; }
    public List<BudgetCategoryDto> Categories { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}