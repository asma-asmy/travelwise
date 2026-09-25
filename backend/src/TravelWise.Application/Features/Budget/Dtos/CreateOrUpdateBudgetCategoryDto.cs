namespace TravelWise.Application.Features.Budget.Dtos;

public class CreateOrUpdateBudgetCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public decimal LimitAmount { get; set; }
}