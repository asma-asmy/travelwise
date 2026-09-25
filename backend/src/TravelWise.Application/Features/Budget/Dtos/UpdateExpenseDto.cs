namespace TravelWise.Application.Features.Budget.Dtos;

public class UpdateExpenseDto
{
    public Guid? BudgetCategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "LKR";
    public string Description { get; set; } = string.Empty;
    public DateTime SpentAt { get; set; }
}