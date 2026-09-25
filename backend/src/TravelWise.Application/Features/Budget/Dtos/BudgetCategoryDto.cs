namespace TravelWise.Application.Features.Budget.Dtos;

public class BudgetCategoryDto
{
    public Guid Id { get; set; }
    public Guid BudgetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal LimitAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public bool IsOverLimit { get; set; }
}