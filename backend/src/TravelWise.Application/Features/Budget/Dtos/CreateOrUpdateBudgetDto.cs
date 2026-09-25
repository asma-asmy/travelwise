namespace TravelWise.Application.Features.Budget.Dtos;

public class CreateOrUpdateBudgetDto
{
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "LKR";
}